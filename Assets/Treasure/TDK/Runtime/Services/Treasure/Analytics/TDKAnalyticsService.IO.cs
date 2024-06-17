#if !UNITY_WEBGL
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using System;
using System.Text;

namespace Treasure
{
    public partial class TDKAnalyticsService : TDKBaseService
    {
        private HttpMessageHandler _httpMessageHandler = null;
        public void SetHttpMessageHandler(HttpMessageHandler h) => _httpMessageHandler = h;
        
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
            using HttpClient client = new HttpClient(_httpMessageHandler);
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
            } catch (OperationCanceledException ex) when (ex.InnerException is TimeoutException tex) {
                TDKLogger.LogWarning($"[TDKAnalyticsService.IO:SendEvents] Failed to send events (timed out): {ex.Message}, {tex.Message}");
            } catch (HttpRequestException ex) {
                TDKLogger.LogWarning($"[TDKAnalyticsService.IO:SendEvents] Failed to send events (http error): {ex.Message}");
            } catch (Exception ex) {
                TDKLogger.LogWarning($"[TDKAnalyticsService.IO:SendEvents] Failed to send events: {ex.Message}");
            }
            return false;
        }
    }
}
#endif
