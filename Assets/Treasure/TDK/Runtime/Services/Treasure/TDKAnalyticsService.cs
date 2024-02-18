using UnityEngine;
using System;
using System.Collections.Generic;
using System.Timers;
using Newtonsoft.Json;
using System.Linq;

namespace Treasure
{
    public partial class TDKAnalyticsService : TDKBaseService
    {
        private List<string> eventCache = new List<string>();
        private Timer flushTimer;

#region lifecycle
        public override void Awake()
        {
            base.Awake();
        }

        public void Start()
        {
            InitPersistentCache();
            
            // initialize flush timer
            flushTimer = new Timer(AnalyticsConstants.CACHE_FLUSH_TIME_SECONDS);
            flushTimer.Elapsed += FlushTimerElapsed;
            flushTimer.AutoReset = true;
            flushTimer.Start();
        }

        private void OnDestroy()
        {
            // stop and dispose of the timer when the service is destroyed
            flushTimer.Stop();
            flushTimer.Dispose();

            cancellationTokenSource.Cancel(); // Cancel the thread when the object is destroyed
            cancellationTokenSource.Dispose();
        }

        private void OnApplicationQuit()
        {
            flushTimer.Stop();
        }
#endregion

#region internal
        private void FlushTimerElapsed(object sender, ElapsedEventArgs e)
        {
            FlushCache();
        }

        private async void FlushCache()
        {
            // copy the cache to avoid modifying it while iterating
            List<string> eventsToFlush = new List<string>(eventCache);

            // clear the cache
            eventCache.Clear();

            // send the batch of events for io
            await SendEvents(eventsToFlush);
        }

        private int CalculateCacheSizeInBytes()
        {
            // calculate the total size of the cached events in bytes
            return eventCache.Sum(e => e.Length);
        }
#endregion

#region public api
        /// <summary>
        /// All other tracking methods should route through this function
        /// </summary>
        /// <param name="eventName">The name of the event</param>
        /// <param name="eventProps">Event properties</param>
        public void TrackCustom(string eventName, Dictionary<string, object> eventProps = null)
        {
            // handle null properties
            if(eventProps == null) {
                eventProps = new Dictionary<string, object>();
            }

            // generate event id
            eventProps[AnalyticsConstants.PROP_EVENT_ID] = Guid.NewGuid().ToString("N");

            // create a dictionary to represent the event
            var eventData = new Dictionary<string, object>
            {
                { AnalyticsConstants.PROP_EVENT_NAME, eventName },
                { AnalyticsConstants.PROP_EVENT_PROPERTIES, eventProps }
            };

            // serialize the event to JSON
            string json = JsonConvert.SerializeObject(eventData);

            // check if adding the event exceeds the cache limits
            if (eventCache.Count + 1 > AnalyticsConstants.MAX_CACHE_EVENT_COUNT || CalculateCacheSizeInBytes() + json.Length > AnalyticsConstants.MAX_CACHE_SIZE_KB)
            {
                FlushCache(); // flush the cache if limits are exceeded
            }

            // add the serialized event to the cache
            eventCache.Add(json);
        }
#endregion
    }
}
