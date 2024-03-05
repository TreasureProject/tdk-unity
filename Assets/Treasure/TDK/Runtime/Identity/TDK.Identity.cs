using Newtonsoft.Json;
using System.Text;
using System.Threading.Tasks;

#if TDK_THIRDWEB
using Thirdweb;
using Nethereum.Siwe.Core;
#endif

using UnityEngine;
using UnityEngine.Networking;

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
        #endregion

        #region public api
        public async Task<string> Authenticate(string projectSlug)
        {
            var project = await TDK.API.GetProjectBySlug(projectSlug);
            var address = await GetWalletAddress();
            var chainId = (int)await GetChainId();

            // Create auth token
            TDKLogger.Log("Generating auth payload");
            var payload = await TDK.API.GetAuthPayload(address, chainId.ToString());

            TDKLogger.Log("Generating auth signature");
            var signature = await GenerateSignature(payload);

            TDKLogger.Log("Logging in smart wallet");
            var token = await TDK.API.LogIn(payload, signature);

#if TDK_THIRDWEB
            // Create session key
            TDKLogger.Log("Creating smart wallet session key");
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

            _authToken = token;
            _isAuthenticated = true;

            return token;
#else
            TDKLogger.LogError("Unable to authenticate. TDK Identity wallet service not implemented.");
            return await Task.FromResult<string>(string.Empty);
#endif
        }

        public void LogOut()
        {
            _authToken = null;
            _isAuthenticated = false;
        }
        #endregion
    }
}
