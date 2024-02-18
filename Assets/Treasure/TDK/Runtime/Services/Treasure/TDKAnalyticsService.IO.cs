using UnityEngine.Networking;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Treasure
{
    public partial class TDKAnalyticsService : TDKBaseService
    {
        private async void SendEvents(List<string> events)
        {
            // Construct the payload by joining all events into a single string
            string payload = string.Join(",", events);

            // Send the payload to the analytics backend via HTTP POST request
            UnityWebRequest request = UnityWebRequest.PostWwwForm("YOUR_BACKEND_URL", payload);
            request.SetRequestHeader("Content-Type", "application/json");

            // Send the request asynchronously
            await request.SendWebRequest();

            // Check if the request was successful
            if (request.result != UnityWebRequest.Result.Success)
            {
                // If the request failed, persist the payload to disk in a separate task
                PersistPayloadToDiskAsync(payload);
            }
            else
            {
                TDKLogger.Log("Events sent successfully");
            }

            // Dispose the request
            request.Dispose();
        }
    }
}
