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
        public API() { }

        public async Task<string> Get(string path)
        {
            var req = new UnityWebRequest
            {
                url = TDK.Instance.AppConfig.TDKApiUrl + path,
                method = "GET",
                downloadHandler = new DownloadHandlerBuffer()
            };
            req.SetRequestHeader("Content-Type", "application/json");
            req.SetRequestHeader("X-Chain-Id", (await TDK.Identity.GetChainId()).ToString());
            if (TDK.Identity.IsAuthenticated)
            {
                req.SetRequestHeader("Authorization", $"Bearer {TDK.Identity.AuthToken}");
            }
            await req.SendWebRequest();

            var rawResponse = req.downloadHandler.text;
            if (req.result != UnityWebRequest.Result.Success)
            {
                throw new UnityException($"[TDK.API.Get] {req.error}: {rawResponse}");
            }

            return rawResponse;
        }

        public async Task<string> Post(string path, string body)
        {
            var req = new UnityWebRequest
            {
                url = TDK.Instance.AppConfig.TDKApiUrl + path,
                method = "POST",
                uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body)),
                downloadHandler = new DownloadHandlerBuffer()
            };
            req.SetRequestHeader("Content-Type", "application/json");
            req.SetRequestHeader("X-Chain-Id", (await TDK.Identity.GetChainId()).ToString());
            if (TDK.Identity.IsAuthenticated)
            {
                req.SetRequestHeader("Authorization", $"Bearer {TDK.Identity.AuthToken}");
            }
            await req.SendWebRequest();

            var rawResponse = req.downloadHandler.text;
            if (req.result != UnityWebRequest.Result.Success)
            {
                throw new UnityException($"[TDK.API.Post] {req.error}: {rawResponse}");
            }

            return rawResponse;
        }
    }
}
