#if UNITY_WEBGL
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;

namespace Treasure
{
    public partial class TDKAnalyticsService : TDKBaseService
    {
        private IEnumerator SendEvent(string eventStr)
        {
            if(eventStr.StartsWith("{"))
            {
                eventStr = "[" + eventStr + "]";
            }

            string baseUrl = TDK.Instance.AppConfig.AnalyticsApiUrl;
            if (!TDK.Instance.AppConfig.AnalyticsApiUrl.EndsWith("/"))
            {
                baseUrl = TDK.Instance.AppConfig.AnalyticsApiUrl + "/";
            }

            using (UnityWebRequest webRequest = UnityWebRequest.PostWwwForm($"{baseUrl}events", eventStr))
            {
                webRequest.SetRequestHeader("Content-Type", "application/json");
                
                // Set the API key header
                webRequest.SetRequestHeader("x-api-key", TDK.Instance.AppConfig.ApiKey);

                yield return webRequest.SendWebRequest();

                if (webRequest.result != UnityWebRequest.Result.Success)
                {
                    TDKLogger.LogWarning($"[TDKAnalyticsService.IO-WebGL:SendEvent] Failed to send event: {webRequest.error}");
                }
                else
                {
                    TDKLogger.Log($"[TDKAnalyticsService.IO-WebGL:SendEvent] Event sent successfully");
                }
            }

            yield return null;
        }
    }
}
#endif
