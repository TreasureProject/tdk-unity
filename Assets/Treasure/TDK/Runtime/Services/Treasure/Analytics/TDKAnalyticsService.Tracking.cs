using System.Collections.Generic;
using Newtonsoft.Json;
using System;

namespace Treasure
{
    public partial class TDKAnalyticsService : TDKBaseService
    {
        private void StartNewSession()
        {
            _sessionId = Guid.NewGuid().ToString("N");
        }

        /// <summary>
        /// All other tracking methods should route through this function
        /// </summary>
        /// <param name="eventName">The name of the event</param>
        /// <param name="eventProps">Event properties</param>
        public async void TrackCustom(string eventName, Dictionary<string, object> eventProps = null, bool highPriority = false)
        {
            // serialize the event to JSON
            string jsonEvtStr = JsonConvert.SerializeObject(BuildBaseEvent(eventName, eventProps));

            // if the event has been flagged as high priority, attempt sending immediately
            if(highPriority) {
                var result = await SendEvent(jsonEvtStr);
                
                // if sending was successful we exit execution...
                if(result) {
                    return;
                }
            }

            // If event is not highPriority or if highPriority send fails, the event enters into event batch cache
            CacheEvent(jsonEvtStr);
        }

        /// <summary>
        /// Flush all pending events to the analytics backend
        /// </summary>
        public async void FlushCache()
        {
            await FlushMemoryCache();
        }
    }
}
