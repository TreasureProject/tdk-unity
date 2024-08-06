using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Treasure
{
    public partial class TDK : MonoBehaviour
    {
        public static API API;

        private void InitAPI()
        {
            API = new API();
        }
    }

    public partial class API
    {
        public struct RequestOverrides
        {
            public string authToken;
            public ChainId chainId;
        }

        public API() { }

        public async Task<string> Get(string path, RequestOverrides? overrides = null)
        {
            // Create request
            var req = new UnityWebRequest
            {
                url = TDK.Instance.AppConfig.TDKApiUrl + path,
                method = "GET",
                downloadHandler = new DownloadHandlerBuffer()
            };
            req.SetRequestHeader("Content-Type", "application/json");

            // Set the API key header
            req.SetRequestHeader("x-api-key", TDK.Instance.AppConfig.ApiKey);

            // Set chain ID with override option
            if (overrides.HasValue && overrides.Value.chainId > 0)
            {
                req.SetRequestHeader("X-Chain-Id", ((int)overrides.Value.chainId).ToString());
            }
            else
            {
                var chainId = (int)await TDK.Connect.GetChainId();
                req.SetRequestHeader("X-Chain-Id", chainId.ToString());
            }

            // Set auth token with override option
            if (overrides.HasValue && !string.IsNullOrEmpty(overrides.Value.authToken))
            {
                req.SetRequestHeader("Authorization", $"Bearer {overrides.Value.authToken}");
            }
            else if (TDK.Identity.IsAuthenticated)
            {
                req.SetRequestHeader("Authorization", $"Bearer {TDK.Identity.AuthToken}");
            }

            // Send request
            await req.SendWebRequest();

            // Read response
            var rawResponse = req.downloadHandler.text;
            if (req.result != UnityWebRequest.Result.Success)
            {
                throw new UnityException($"[TDK.API.Get] {req.error}{(rawResponse.Length > 0 ? ": " + rawResponse : " (no response)")}");
            }

            return rawResponse;
        }

        public async Task<string> Post(string path, string body, RequestOverrides? overrides = null)
        {
            // Create request
            var req = new UnityWebRequest
            {
                url = TDK.Instance.AppConfig.TDKApiUrl + path,
                method = "POST",
                uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body)),
                downloadHandler = new DownloadHandlerBuffer()
            };
            req.SetRequestHeader("Content-Type", "application/json");

            // Set the API key header
            req.SetRequestHeader("x-api-key", TDK.Instance.AppConfig.ApiKey);

            // Set chain ID with override option
            if (overrides.HasValue && overrides.Value.chainId > 0)
            {
                req.SetRequestHeader("X-Chain-Id", ((int)overrides.Value.chainId).ToString());
            }
            else
            {
                var chainId = (int)await TDK.Connect.GetChainId();
                req.SetRequestHeader("X-Chain-Id", chainId.ToString());
            }

            // Set auth token with override option
            if (overrides.HasValue && !string.IsNullOrEmpty(overrides.Value.authToken))
            {
                req.SetRequestHeader("Authorization", $"Bearer {overrides.Value.authToken}");
            }
            else if (TDK.Identity.IsAuthenticated)
            {
                req.SetRequestHeader("Authorization", $"Bearer {TDK.Identity.AuthToken}");
            }

            // Send request
            await req.SendWebRequest();

            // Read response
            var rawResponse = req.downloadHandler.text;
            if (req.result != UnityWebRequest.Result.Success)
            {
                throw new UnityException($"[TDK.API.Post] {req.error}: {rawResponse}");
            }

            return rawResponse;
        }
    }
}
