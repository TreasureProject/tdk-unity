using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;

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
        private IEnumerator SendPersistedEventsRoutine(string payload, string filePath)
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

                using (UnityWebRequest webRequest = UnityWebRequest.PostWwwForm(AnalyticsConstants.API_ENDPOINT, payload))
                {
                    // send the request and wait for a response
                    yield return webRequest.SendWebRequest();

                    if (webRequest.result != UnityWebRequest.Result.Success)
                    {
                        TDKLogger.Log($"[TDKAnalyticsService.IO:SendPersistedEventsRoutine] Failed to send persisted event batch {fileName}: {webRequest.error}");
                        
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

        private async Task SendEvents(List<string> events)
        {
            // construct the payload by joining all events into a single string
            string payload = string.Join(",", events);

            // send the payload to the analytics backend via HTTP POST request
            UnityWebRequest request = UnityWebRequest.PostWwwForm(AnalyticsConstants.API_ENDPOINT, payload);
            request.SetRequestHeader("Content-Type", "application/json");

            // send the request asynchronously
            await request.SendWebRequest();

            // Check if the request was successful
            if (request.result != UnityWebRequest.Result.Success)
            {
                TDKLogger.Log("[TDKAnalyticsService.IO:SendEvents] Failed to send events: " + request.error);

                // if the request failed, persist the payload to disk in a separate task
                PersistPayloadToDiskAsync(payload);
            }
            else
            {
                TDKLogger.Log("[TDKAnalyticsService.IO:SendEvents] Events sent successfully");
            }

            // dispose the request
            request.Dispose();
        }
    }
}
