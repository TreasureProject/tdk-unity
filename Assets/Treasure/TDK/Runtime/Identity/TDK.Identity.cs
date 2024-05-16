using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using System;
using System.Collections.Generic;

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
        }
    }

    public class Identity
    {
        #region private vars
        private Dictionary<ChainId, Project> _chainIdToProject = new Dictionary<ChainId, Project>();
        private string _address;
        private string _authToken;
        #endregion

        #region accessors / mutators
        public string Address
        {
            // Identity service can have its own address set in the case where an existing session was used and no wallet is connected
            // Otherwise, fall back to Connect service address
            get
            {
                if (string.IsNullOrEmpty(_address))
                {
                    _address = TDK.Connect.Address;
                }

                return _address;
            }
        }

        public string AuthToken
        {
            get { return _authToken; }
        }

        public bool IsAuthenticated
        {
            get { return !string.IsNullOrEmpty(_authToken); }
        }
        #endregion

        #region constructors
        public Identity() { }
        #endregion

        #region private methods
        private async Task<Project> GetProjectByChainId(ChainId chainId)
        {
            if (_chainIdToProject.ContainsKey(chainId))
            {
                return _chainIdToProject[chainId];
            }

            var project = await TDK.API.GetProjectBySlug(TDK.Instance.AppConfig.CartridgeTag, new API.RequestOverrides { chainId = chainId });
            _chainIdToProject[chainId] = project;
            return project;
        }

        private async Task<string> SignLoginPayload(AuthPayload payload)
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
            return await TDKServiceLocator.GetService<TDKThirdwebService>().Wallet.Sign(finalMessage);
#else
            TDKLogger.LogError("Unable to sign login payload. TDK Identity wallet service not implemented.");
            return await Task.FromResult<string>(string.Empty);
#endif
        }

        private async Task CreateSessionKey(Project project)
        {
#if TDK_THIRDWEB
            var permissionEndTimestamp = (decimal)(Utils.GetUnixTimeStampNow() + 60 * 60 * 24 * TDK.Instance.AppConfig.SessionLengthDays);
            await TDKServiceLocator.GetService<TDKThirdwebService>().Wallet.CreateSessionKey(
                signerAddress: project.backendWallet,
                approvedTargets: project.callTargets,
                nativeTokenLimitPerTransactionInWei: "0",
                permissionStartTimestamp: "0",
                permissionEndTimestamp: permissionEndTimestamp.ToString(),
                reqValidityStartTimestamp: "0",
                reqValidityEndTimestamp: permissionEndTimestamp.ToString()
            );
#else
            TDKLogger.LogError("Unable to create session key. TDK Identity wallet service not implemented.");
            return await Task.FromResult<string>(string.Empty);
#endif
        }
        #endregion

        #region public api
        public async Task<User?> ValidateUserSession(ChainId chainId, string authToken)
        {
            TDKLogger.Log("Validating existing user session");
            var project = await GetProjectByChainId(chainId);
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

                _address = user.smartAccountAddress;
                _authToken = authToken;

                TDKLogger.Log("Existing user session is valid");

                return user;
            }
            catch
            {
                // Auth token was invalid or expired
                return null;
            }
        }

        public async Task<string> StartUserSession(ChainId sessionChainId = ChainId.Unknown, string sessionAuthToken = null)
        {
            // Check if user already has a valid session for the specified chain
            var currentChainId = await TDK.Connect.GetChainId();
            var chainId = sessionChainId == ChainId.Unknown ? currentChainId : sessionChainId;
            var authToken = !string.IsNullOrEmpty(sessionAuthToken) ? sessionAuthToken : _authToken;
            if (!string.IsNullOrEmpty(authToken))
            {
                var user = await ValidateUserSession(chainId, authToken);
                if (user.HasValue)
                {
                    TDKLogger.Log("User already has a valid session");
                    return authToken;
                }
            }

#if TDK_THIRDWEB
            // The rest of this flow requires a wallet to be connected
            if (!await TDK.Connect.IsWalletConnected())
            {
                TDKLogger.LogError("Unable to start user session. TDK Identity wallet not connected.");
                return await Task.FromResult(string.Empty);
            }

            // Switch chains if not on the correct one already
            if (chainId != currentChainId)
            {
                TDKLogger.Log($"Switching chain to {chainId}");
                await TDK.Connect.SetChainId(chainId);
            }

            var project = await GetProjectByChainId(chainId);
            var didCreateSession = false;

            // If smart wallet isn't deployed yet, create a new session to bundle the two txs
            if (!await TDKServiceLocator.GetService<TDKThirdwebService>().Wallet.IsDeployed())
            {
                TDKLogger.Log("Deploying smart wallet and creating session key");
                await CreateSessionKey(project);
                didCreateSession = true;
            }

            // Create auth token
            TDKLogger.Log("Fetching login payload");
            var payload = await TDK.API.GetLoginPayload(Address);

            TDKLogger.Log("Signing login payload");
            var signature = await SignLoginPayload(payload);

            TDKLogger.Log("Logging in and fetching TDK auth token");
            _authToken = await TDK.API.LogIn(payload, signature);

            // Smart wallet was already deployed, so check for existing sessions
            if (!didCreateSession)
            {
                var backendWallet = project.backendWallet;
                var requestedCallTargets = project.requestedCallTargets;
                var activeSigners = await TDKServiceLocator.GetService<TDKThirdwebService>().Wallet.GetAllActiveSigners();

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

            TDKLogger.Log("User session started successfully");

            return _authToken;
#else
            TDKLogger.LogError("Unable to start user session. TDK Identity wallet service not implemented.");
            return await Task.FromResult(string.Empty);
#endif
        }

        public async void EndUserSession()
        {
            try
            {
                await TDK.Connect.Disconnect(true);
            }
            catch (Exception e)
            {
                TDKLogger.LogError($"Error ending user session: {e}");
                return;
            }

            _address = null;
            _authToken = null;
        }
        #endregion
    }
}
