using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;

namespace Treasure
{
    /// <summary>
    /// This method does not adhere to separation of concerns as it modifies the cache. This is 
    /// a workaround to enable invocation from a ThreadGroup to [this] coroutine and the need
    /// for accessing the unity api (Application.internetReachability & UnityWebRequest) via
    /// the main tread.
    /// </summary>
    public partial class TDKAnalyticsService : TDKBaseService
    {
        private IEnumerator SendPersistedEventsRoutine(string payload, string filePath)
        {
            // if there is no intenet connection, we skip this and keep persisted batch
            if(Application.internetReachability != NetworkReachability.NotReachable)
            {
                // retrieve file name and send attempts value
                var fileName = Path.GetFileName(filePath);
                int numSendAttempts = PlayerPrefs.GetInt(fileName + "_sendattemps", 0);

                // delete the file if max send attempts have been reached & stop processing
                if(numSendAttempts > AnalyticsConstants.PERSISTENT_MAX_RETRIES) {
                    File.Delete(filePath);
                    yield return null;
                }

                using (UnityWebRequest webRequest = UnityWebRequest.PostWwwForm(AnalyticsConstants.API_ENDPOINT, payload))
                {
                    // send the request and wait for a response
                    yield return webRequest.SendWebRequest();

                    if (webRequest.result != UnityWebRequest.Result.Success)
                    {
                        TDKLogger.Log("[TDKAnalyticsService.IO:SendPersistedEventsRoutine] Failed to send persisted event batch: " + webRequest.error);
                        
                        // increment playerprefs send attempt
                        PlayerPrefs.SetInt(fileName + "_sendattemps", numSendAttempts + 1);
                    }
                    else
                    {
                        TDKLogger.Log("[TDKAnalyticsService.IO:SendPersistedEventsRoutine] Persisted event batch sent successfully");
                        File.Delete(filePath); // Delete file after successful processing
                        PlayerPrefs.DeleteKey(fileName + "_sendattemps");
                    }
                    PlayerPrefs.Save();
                }
            }
            yield return null;
        }

        private async Task SendEvents(List<string> events)
        {
            // Construct the payload by joining all events into a single string
            string payload = string.Join(",", events);

            // Send the payload to the analytics backend via HTTP POST request
            UnityWebRequest request = UnityWebRequest.PostWwwForm(AnalyticsConstants.API_ENDPOINT, payload);
            request.SetRequestHeader("Content-Type", "application/json");

            // Send the request asynchronously
            await request.SendWebRequest();

            // Check if the request was successful
            if (request.result != UnityWebRequest.Result.Success)
            {
                TDKLogger.Log("[TDKAnalyticsService.IO:SendEvents] Failed to send events: " + request.error);

                // If the request failed, persist the payload to disk in a separate task
                PersistPayloadToDiskAsync(payload);
            }
            else
            {
                TDKLogger.Log("[TDKAnalyticsService.IO:SendEvents] Events sent successfully");
            }

            // Dispose the request
            request.Dispose();
        }
    }
}
