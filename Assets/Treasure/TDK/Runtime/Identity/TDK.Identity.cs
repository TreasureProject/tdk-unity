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
        private ChainId _chainId = ChainId.Unknown;
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
            if (_chainId == ChainId.Unknown)
            {
                _chainId = (ChainId)(int)await _wallet.GetChainId();
            }

            return _chainId;
#else
            TDKLogger.LogError("Unable to retrieve chain ID. TDK Identity wallet service not implemented.");
            return await Task.FromResult(ChainId.Arbitrum);
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
        #endregion

        #region public api
        public void LogOut()
        {
            _authToken = null;
            _isAuthenticated = false;
        }

        public async Task<string> Authenticate(string projectSlug)
        {
#if TDK_THIRDWEB
            var project = await TDK.API.GetProjectBySlug(projectSlug);
            var address = await GetWalletAddress();
            var chainId = (int)await GetChainId();
            var backendWallet = project.backendWallets[0].ToLowerInvariant();
            var requestedCallTargets = project.callTargets.Select(callTarget => callTarget.ToLowerInvariant());
            var didCreateSession = false;
            var hasActiveSession = false;
            try
            {
                var activeSigners = await _wallet.GetAllActiveSigners();
                hasActiveSession = activeSigners.Any((signer) =>
                {
                    var signerCallTargets = signer.permissions.approvedCallTargets.Select(callTarget => callTarget.ToLowerInvariant());
                    return signer.signer.ToLowerInvariant() == backendWallet &&
                        requestedCallTargets.All(callTarget => signerCallTargets.Contains(callTarget));
                });
                if (hasActiveSession)
                {
                    TDKLogger.Log("Using existing session key");
                }
            }
            catch
            {
                // Call can expectedly throw if smart wallet is not deployed already
            }

            // Create session (and deploy smart wallet if undeployed)
            if (!didCreateSession && !hasActiveSession)
            {
                TDKLogger.Log("Creating new session key");
                var permissionEndTimestamp = (decimal)(Utils.GetUnixTimeStampNow() + 60 * 60 * 24 * TDK.Instance.AppConfig.SessionLengthDays);
                await _wallet.CreateSessionKey(
                    signerAddress: project.backendWallets[0],
                    approvedTargets: project.callTargets,
                    nativeTokenLimitPerTransactionInWei: "0",
                    permissionStartTimestamp: "0",
                    permissionEndTimestamp: permissionEndTimestamp.ToString(),
                    reqValidityStartTimestamp: "0",
                    reqValidityEndTimestamp: permissionEndTimestamp.ToString()
                );
            }

            // Create auth token
            TDKLogger.Log("Generating auth payload");
            var payload = await TDK.API.GetLoginPayload(address);

            TDKLogger.Log("Generating auth signature");
            var signature = await GenerateSignature(payload);

            TDKLogger.Log("Logging in smart wallet");
            var token = await TDK.API.LogIn(payload, signature);

            _authToken = token;
            _isAuthenticated = true;

            return token;
#else
            TDKLogger.LogError("Unable to authenticate. TDK Identity wallet service not implemented.");
            return await Task.FromResult<string>(string.Empty);
#endif
        }
        #endregion
    }
}
