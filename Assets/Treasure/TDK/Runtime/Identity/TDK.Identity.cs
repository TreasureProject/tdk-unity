using Nethereum.Siwe.Core;
using Newtonsoft.Json;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Thirdweb;
using UnityEngine;
using UnityEngine.Networking;

namespace Treasure
{
    public partial class TDK : MonoBehaviour
    {
        public static Identity Identity;

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
        private Wallet _wallet
        {
            get
            {
#if TDK_THIRDWEB
                return TDKServiceLocator.GetService<TDKThirdwebService>().Wallet;
#else
                return null;
#endif
            }
        }

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
            return await _wallet.GetAddress();
        }

        public async Task<ChainId> GetChainId()
        {
            var chainId = await _wallet.GetChainId();
            return chainId.Equals(BigInteger.Parse(ChainId.ArbitrumSepolia.ToString())) ? ChainId.ArbitrumSepolia : ChainId.Arbitrum;
        }
        #endregion

        #region constructors
        public Identity() { }
        #endregion

        #region private methods
        private async Task<TDKAuthPayload> GetAuthPayload()
        {
            var body = JsonConvert.SerializeObject(new TDKAuthPayloadRequest
            {
                address = await _wallet.GetAddress(),
                chainId = (await _wallet.GetChainId()).ToString(),
            });
            var req = new UnityWebRequest
            {
                url = $"{TDK.Instance.AppConfig.TDKApiUrl}/auth/payload",
                method = "POST",
                uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body)),
                downloadHandler = new DownloadHandlerBuffer()
            };
            req.SetRequestHeader("Content-Type", "application/json");
            await req.SendWebRequest();

            var rawResponse = req.downloadHandler.text;
            if (req.result != UnityWebRequest.Result.Success)
            {
                throw new UnityException($"[GetAuthPayload] {req.error}: {rawResponse}");
            }

            var response = JsonConvert.DeserializeObject<TDKAuthPayloadResponse>(rawResponse);
            return response.payload;
        }

        private async Task<string> GenerateSignature(TDKAuthPayload payload)
        {
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

#if TDK_THIRDWEB
            return await TDKServiceLocator.GetService<TDKThirdwebService>().Sign(finalMessage);
#else
            return await Task.FromResult<string>(string.Empty);
#endif
        }

        private async Task<string> LogIn(TDKAuthPayload payload, string signature)
        {
            var body = JsonConvert.SerializeObject(new TDKAuthLoginRequest()
            {
                payload = new TDKAuthLoginPayload()
                {
                    payload = payload,
                    signature = signature
                },
            });
            var req = new UnityWebRequest
            {
                url = $"{TDK.Instance.AppConfig.TDKApiUrl}/auth/login",
                method = "POST",
                uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body)),
                downloadHandler = new DownloadHandlerBuffer()
            };
            req.SetRequestHeader("Content-Type", "application/json");
            await req.SendWebRequest();

            var rawResponse = req.downloadHandler.text;
            if (req.result != UnityWebRequest.Result.Success)
            {
                throw new UnityException($"[LogIn] {req.error}: {rawResponse}");
            }

            var response = JsonConvert.DeserializeObject<TDKAuthLoginResponse>(rawResponse);
            return response.token;
        }
        #endregion

        #region public api
        public async Task<string> Authenticate(string projectSlug)
        {
            var project = await TDK.API.GetProjectBySlug(projectSlug);

            // Create auth token
            var payload = await GetAuthPayload();
            var signature = await GenerateSignature(payload);
            var token = await LogIn(payload, signature);

            // Create session key
            var permissionEndTimestamp = (decimal)(Utils.GetUnixTimeStampNow() + 60 * 60 * 24 * TDK.Instance.AppConfig.SessionLengthDays);
            await _wallet.CreateSessionKey(
                signerAddress: project.backendWallets[0],
                approvedTargets: project.callTargets,
                nativeTokenLimitPerTransactionInWei: "0",
                permissionStartTimestamp: "0",
                permissionEndTimestamp: permissionEndTimestamp.ToString(),
                reqValidityStartTimestamp: "0",
                reqValidityEndTimestamp: Utils.GetUnixTimeStampIn10Years().ToString()
            );

            _authToken = token;
            _isAuthenticated = true;

            return token;
        }

        public void LogOut()
        {
            _authToken = null;
            _isAuthenticated = false;
        }
        #endregion
    }
}
