using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System;
using System.Text;

namespace Treasure
{
    public partial class TDKAnalyticsService : TDKBaseService
    {
        /// <summary>
        /// This method does not adhere to separation of concerns as it modifies the cache. This is 
        /// a workaround to enable invocation from a ThreadGroup to [this] coroutine and the need
        /// for accessing the unity api (Application.internetReachability & UnityWebRequest) via
        /// the main tread. 
        /// </summary>
        /// <returns></returns>
        private IEnumerator SendPersistedEventBatchRoutine(string payload, string filePath)
        {
            // if there is no intenet connection, we skip this and keep persisted batch
            if(Application.internetReachability != NetworkReachability.NotReachable)
            {
                // retrieve file name and send attempts value
                string fileName = Path.GetFileName(filePath);
                string playerPrefsKey = fileName + "_sendattemps";
                int numSendAttempts = PlayerPrefs.GetInt(playerPrefsKey, 0);

                // delete the file if max send attempts have been reached & stop processing
                if(numSendAttempts > AnalyticsConstants.PERSISTENT_MAX_RETRIES) {
                    RemovePersistedBatch(filePath, playerPrefsKey);
                    yield return null;
                }

                using (UnityWebRequest webRequest = UnityWebRequest.PostWwwForm($"{TDK.Instance.AppConfig.AnalyticsApiUrl}/events", payload))
                {
                    webRequest.SetRequestHeader("Content-Type", "application/json");

                    // send the request and wait for a response
                    yield return webRequest.SendWebRequest();

                    if (webRequest.result != UnityWebRequest.Result.Success)
                    {
                        TDKLogger.LogWarning($"[TDKAnalyticsService.IO:SendPersistedEventsRoutine] Failed to send persisted event batch {fileName}: {webRequest.error}");
                        
                        // increment playerprefs send attempt
                        PlayerPrefs.SetInt(playerPrefsKey, numSendAttempts + 1);
                    }
                    else
                    {
                        TDKLogger.Log($"[TDKAnalyticsService.IO:SendPersistedEventsRoutine] Persisted event batch ({fileName}) sent successfully");
                        RemovePersistedBatch(filePath, playerPrefsKey);
                    }
                    PlayerPrefs.Save();
                }
            }
            yield return null;
        }

        /// <summary>
        /// See SendPersistedEventsRoutine doc
        /// </summary>
        private void RemovePersistedBatch(string filePath, string key)
        {
            File.Delete(filePath);
            PlayerPrefs.DeleteKey(key);
        }

        private async Task<bool> SendEvent(string eventStr)
        {
            return await SendEventBatch(new List<string>() { eventStr });
        }
        
        private async Task<bool> SendEventBatch(List<string> events)
        {
            // construct the payload by joining all events into a single string
            string payload = "[" + string.Join(",", events) + "]";
            TDKLogger.Log("[TDKAnalyticsService.IO:SendEvents] Payload:" + payload);

            // send the payload to the analytics backend via HTTP POST request
            using (HttpClient client = new HttpClient())
            {
                try {
                    // Set the base address ensuring trailing slash
                    string baseAddress = TDK.Instance.AppConfig.AnalyticsApiUrl;
                    if (!TDK.Instance.AppConfig.AnalyticsApiUrl.EndsWith("/"))
                    {
                        baseAddress = TDK.Instance.AppConfig.AnalyticsApiUrl + "/";
                    }
                    client.BaseAddress = new Uri(baseAddress);

                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "events")
                    {
                        Content = new StringContent(payload, Encoding.UTF8, "application/json"),
                        Headers = { { "Accept", "application/json" } }
                    };

                    HttpResponseMessage response = await client.SendAsync(request);
                    
                    // Ensure the request was successful
                    response.EnsureSuccessStatusCode();

                    // Read the response content
                    TDKLogger.Log("[TDKAnalyticsService.IO:SendEvents] Events sent successfully");

                    return true;
                }
                catch (HttpRequestException e)
                {
                    TDKLogger.LogWarning("[TDKAnalyticsService.IO:SendEvents] Failed to send events: " + e.Message);
                    return false;
                }
            }
        }
    }
}
