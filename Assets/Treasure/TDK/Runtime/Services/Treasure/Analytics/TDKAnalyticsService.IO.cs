using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using System;
using System.Text;

namespace Treasure
{
    public partial class TDKAnalyticsService : TDKBaseService
    {
        private async Task<bool> SendEvent(string eventStr)
        {
            return await SendEventBatch(new List<string>() { eventStr });
        }
        
        private async Task<bool> SendEventBatch(List<string> events)
        {
            // forward the payload by joining all events into a single string
            return await SendEventBatch(string.Join(",", events));
        }
        
        private async Task<bool> SendEventBatch(string payload)
        {
            if(payload.StartsWith("{"))
            {
                payload = "[" + payload + "]";
            }
            // TDKLogger.Log("[TDKAnalyticsService.IO:SendEvents] Payload:" + payload);

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
                    TDKLogger.LogWarning("[TDKAnalyticsService.IO:SendEvents] " + e.Message);
                    return false;
                }
            }
        }
    }
}
