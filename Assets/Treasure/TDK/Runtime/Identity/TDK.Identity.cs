using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using System;

#if TDK_THIRDWEB
using Thirdweb;
using Nethereum.Siwe.Core;
#endif

namespace Treasure
{
    public partial class TDK : MonoBehaviour
    {
        public static Identity Identity { get; private set; }

        /// <summary>
        /// Initialize the Identity module
        /// </summary>
        private void InitIdentity()
        {
            Identity = new Identity();

#if TDK_THIRDWEB
            TDKServiceLocator.GetService<TDKThirdwebService>();
#endif
        }
    }

    public class Identity
    {
        #region private vars
        private string _authToken;
        private bool _isAuthenticated;
        #endregion

        public UnityEvent<string> OnConnected = new UnityEvent<string>();
        public UnityEvent<Exception> OnConnectionError = new UnityEvent<Exception>();
        public UnityEvent OnDisconnected = new UnityEvent();

        #region accessors / mutators
#if TDK_THIRDWEB
        private Wallet _wallet
        {
            get
            {
                return TDKServiceLocator.GetService<TDKThirdwebService>().Wallet;
            }
        }
#endif

        public string AuthToken
        {
            get { return _authToken; }
        }

        public bool IsAuthenticated
        {
            get { return _isAuthenticated; }
        }

        public async Task<string> GetWalletAddress()
        {
#if TDK_THIRDWEB
            return await _wallet.GetAddress();
#else
            TDKLogger.LogError("Unable to retrieve wallet address. TDK Identity wallet service not implemented.");
            return await Task.FromResult<string>(string.Empty);
#endif
        }

        public async Task<ChainId> GetChainId()
        {
#if TDK_THIRDWEB
            var chainId = (int)await _wallet.GetChainId();
            return chainId == (int)ChainId.ArbitrumSepolia ? ChainId.ArbitrumSepolia : ChainId.Arbitrum;
#else
            TDKLogger.LogError("Unable to retrieve chain ID. TDK Identity wallet service not implemented.");
            return await Task.FromResult<ChainId>(ChainId.Arbitrum);
#endif

        }
        #endregion

        #region constructors
        public Identity() { }
        #endregion

        #region private methods
        private async Task<string> GenerateSignature(AuthPayload payload)
        {
#if TDK_THIRDWEB
            var message = new SiweMessage()
            {
                Uri = payload.uri,
                Statement = payload.statement,
                Address = payload.address,
                Domain = payload.domain,
                ChainId = payload.chain_id,
                Version = payload.version,
                Nonce = payload.nonce,
                IssuedAt = payload.issued_at,
                ExpirationTime = payload.expiration_time,
                NotBefore = payload.invalid_before,
            };
            var finalMessage = SiweMessageStringBuilder.BuildMessage(message);

            return await TDKServiceLocator.GetService<TDKThirdwebService>().Sign(finalMessage);
#else
            TDKLogger.LogError("Unable to generate signature. TDK Identity wallet service not implemented.");
            return await Task.FromResult<string>(string.Empty);
#endif
        }

        private async Task CreateSessionKey(Project project)
        {
            var permissionEndTimestamp = (decimal)(Utils.GetUnixTimeStampNow() + 60 * 60 * 24 * TDK.Instance.AppConfig.SessionLengthDays);
            await _wallet.CreateSessionKey(
                signerAddress: project.backendWallet,
                approvedTargets: project.callTargets,
                nativeTokenLimitPerTransactionInWei: "0",
                permissionStartTimestamp: "0",
                permissionEndTimestamp: permissionEndTimestamp.ToString(),
                reqValidityStartTimestamp: "0",
                reqValidityEndTimestamp: permissionEndTimestamp.ToString()
            );
        }
        #endregion

        #region public api
        public void LogOut()
        {
            _authToken = null;
            _isAuthenticated = false;
        }

        public async Task<User?> ValidateUserSession(Project project, string authToken, int chainId)
        {
            TDKLogger.Log("Validating existing user session");

            var backendWallet = project.backendWallet;
            var requestedCallTargets = project.requestedCallTargets;

            try
            {
                // Fetch user details for provided auth token
                TDKLogger.Log("Fetching user details");
                var user = await TDK.API.GetCurrentUser(new API.RequestOverrides()
                {
                    authToken = authToken,
                    chainId = chainId
                });

                // Check if any active signers match the requested projects' call targets
                var hasActiveSession = user.allActiveSigners.Any((signer) =>
                {
                    return signer.signer.ToLowerInvariant() == backendWallet &&
                        requestedCallTargets.All(callTarget => signer.approvedTargets.Contains(callTarget));
                });

                if (!hasActiveSession)
                {
                    TDKLogger.Log("Existing user session does not have required permissions. User must start a new session.");
                    return null;
                }

                _authToken = authToken;
                _isAuthenticated = true;

                TDKLogger.Log("Existing user session is valid");

                return user;
            }
            catch
            {
                // Auth token was invalid or expired
                return null;
            }
        }

        public async Task<string> StartUserSession(Project project)
        {
            TDKLogger.Log("Starting new user session");

#if TDK_THIRDWEB
            var address = await _wallet.GetAddress();
            if (address == null)
            {
                TDKLogger.LogError("Unable to start user session. TDK Identity wallet not connected.");
                return await Task.FromResult(string.Empty);
            }

            var didCreateSession = false;

            // If smart wallet isn't deployed yet, create a new session to bundle the two txs
            if (!await _wallet.IsDeployed())
            {
                TDKLogger.Log("Deploying smart wallet and creating session key");
                await CreateSessionKey(project);
                didCreateSession = true;
            }

            // Create auth token
            TDKLogger.Log("Fetching login payload");
            var payload = await TDK.API.GetLoginPayload(address);

            TDKLogger.Log("Signing login payload");
            var signature = await GenerateSignature(payload);

            TDKLogger.Log("Logging in and fetching TDK auth token");
            var token = await TDK.API.LogIn(payload, signature);

            // Smart wallet was already deployed, so check for existing sessions
            if (!didCreateSession)
            {
                var backendWallet = project.backendWallet;
                var requestedCallTargets = project.requestedCallTargets;
                var activeSigners = await _wallet.GetAllActiveSigners();

                // Check if any active signers match the requested projects' call targets
                var hasActiveSession = activeSigners.Any((signer) =>
                {
                    var signerCallTargets = signer.permissions.approvedCallTargets.Select(callTarget => callTarget.ToLowerInvariant());
                    return signer.signer.ToLowerInvariant() == backendWallet &&
                        requestedCallTargets.All(callTarget => signerCallTargets.Contains(callTarget));
                });

                if (!hasActiveSession)
                {
                    TDKLogger.Log("Creating new session key");
                    await CreateSessionKey(project);
                }
                else
                {
                    TDKLogger.Log("Using existing session key");
                }
            }

            _authToken = token;
            _isAuthenticated = true;

            TDKLogger.Log("User session started successfully");

            return token;
#else
            TDKLogger.LogError("Unable to start user session. TDK Identity wallet service not implemented.");
            return await Task.FromResult(string.Empty);
#endif
        }
        #endregion
    }
}
