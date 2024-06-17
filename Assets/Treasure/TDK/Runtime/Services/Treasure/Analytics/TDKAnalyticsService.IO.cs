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
            bool success = false;

            try {
                using HttpClient client = _httpMessageHandler != null ? new HttpClient(_httpMessageHandler) : new HttpClient();
                if(payload.StartsWith("{"))
                {
                    payload = "[" + payload + "]";
                }
                // TDKLogger.Log("[TDKAnalyticsService.IO:SendEvents] Payload:" + payload);

                // Set the base address ensuring trailing slash
                string baseAddress = TDK.Instance.AppConfig.AnalyticsApiUrl;
                if (!TDK.Instance.AppConfig.AnalyticsApiUrl.EndsWith("/"))
                {
                    baseAddress = TDK.Instance.AppConfig.AnalyticsApiUrl + "/";
                }
                client.BaseAddress = new Uri(baseAddress);
                client.DefaultRequestHeaders.Add("Accept", "application/json");

                // send the payload to the analytics backend via HTTP POST request
                HttpResponseMessage response = await client.PostAsync(
                    "events",
                    new StringContent(payload, Encoding.UTF8, "application/json")
                );
                // Ensure the request was successful
                response.EnsureSuccessStatusCode();
                TDKLogger.Log("[TDKAnalyticsService.IO:SendEvents] Events sent successfully");
                success = true;
            } catch (OperationCanceledException ex) when (ex.InnerException is TimeoutException tex) {
                TDKLogger.LogWarning($"[TDKAnalyticsService.IO:SendEvents] Failed to send events (timed out): {ex.Message}, {tex.Message}");
            } catch (HttpRequestException ex) {
                TDKLogger.LogWarning($"[TDKAnalyticsService.IO:SendEvents] Failed to send events (http error): {ex.Message}");
            } catch (Exception ex) {
                TDKLogger.LogError($"[TDKAnalyticsService.IO:SendEvents] Failed to send events: {ex.Message}");
            }
            return success;
        }
    }
}
#endif
