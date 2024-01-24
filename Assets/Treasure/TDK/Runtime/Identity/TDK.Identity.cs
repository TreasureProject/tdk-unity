using Nethereum.Siwe.Core;
using Newtonsoft.Json;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Treasure
{
    public partial class TDK : MonoBehaviour
    {
        public static Identity identity;

        /// <summary>
        /// Initialize the Identity module
        /// </summary>
        private void InitIdentity()
        {
            identity = new Identity(AppConfig.TDKApiUrl);

#if TDK_THIRDWEB
            TDKServiceLocator.GetService<TDKThirdwebService>();
#endif
        }
    }

    public class Identity
    {
        private string _tdkApiUrl;
        private string _authToken;

        public Identity(string tdkApiUrl)
        {
            _tdkApiUrl = tdkApiUrl;
        }

        public string AuthToken
        {
            get { return _authToken; }
        }

        public async Task<string> GetAddress()
        {
            return await TDKServiceLocator.GetService<TDKThirdwebService>().GetAddress();
        }

        public async Task<BigInteger> GetChainId()
        {
            return await TDKServiceLocator.GetService<TDKThirdwebService>().GetChainId();
        }

        private async Task<TDKAuthPayload> GetAuthPayload()
        {
            var body = JsonConvert.SerializeObject(new TDKAuthPayloadRequest
            {
                address = await GetAddress(),
                chainId = (await GetChainId()).ToString(),
            });
            var req = new UnityWebRequest
            {
                url = $"{_tdkApiUrl}/auth/payload",
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
            return await TDKServiceLocator.GetService<TDKThirdwebService>().Sign(finalMessage);
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
                url = $"{_tdkApiUrl}/auth/login",
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

        public async Task<string> Authenticate()
        {
            var payload = await GetAuthPayload();
            var signature = await GenerateSignature(payload);
            var token = await LogIn(payload, signature);
            _authToken = token;
            return token;
        }
    }
}
