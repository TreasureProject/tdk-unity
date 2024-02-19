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
        private IEnumerator SendPersistedEventsRoutine(string payload, string filePath)
        {
            if(Application.internetReachability != NetworkReachability.NotReachable)
            {
                using (UnityWebRequest webRequest = UnityWebRequest.PostWwwForm(AnalyticsConstants.API_ENDPOINT, payload))
                {
                    // Send the request and wait for a response
                    yield return webRequest.SendWebRequest();

                    if (webRequest.result != UnityWebRequest.Result.Success)
                    {
                        TDKLogger.Log("[TDKAnalyticsService.IO:SendPersistedEventsRoutine] Failed to send persisted event batch: " + webRequest.error);
                    }
                    else
                    {
                        TDKLogger.Log("[TDKAnalyticsService.IO:SendPersistedEventsRoutine] Persisted event batch sent successfully");
                        File.Delete(filePath); // Delete file after successful processing
                    }
                }
            }
            yield return null;
        }

        private async Task SendEvents(List<string> events)
        {
            // Construct the payload by joining all events into a single string
            string payload = string.Join(",", events);

            // Send the payload to the analytics backend via HTTP POST request
            UnityWebRequest request = UnityWebRequest.PostWwwForm(AnalyticsConstants.API_ENDPOINT + "abc", payload);
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
