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

            // ...if highPriority send fails, the event enters into event batch cache

            // check if adding the event exceeds the cache limits
            if(_eventCache.Count + 1 > AnalyticsConstants.MAX_CACHE_EVENT_COUNT || CalculateCacheSizeInBytes() + jsonEvtStr.Length > AnalyticsConstants.MAX_CACHE_SIZE_KB * 1024)
            {
                // flush the cache if limits are exceeded
                FlushCache(); 
                
                // restart flush coroutine
                StopCoroutine(FlushTimer());
                StartCoroutine(FlushTimer());
            }

            // add the serialized event to the cache
            _eventCache.Add(jsonEvtStr);
        }
    }
}
