using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using System.Threading.Tasks;

namespace Treasure
{
    public partial class TDKAnalyticsService : TDKBaseService
    {
        public const uint EventsVersion = 1;

        private Dictionary<string, object> deviceInfo;
        private List<string> eventCache = new List<string>();

#region lifecycle
        public override void Awake()
        {
            base.Awake();
            BuildDeviceInfo();
        }

        public void Start()
        {
            InitPersistentCache();

            // start coroutine for event flushing
            StartCoroutine(FlushTimer());
        }

        private void OnDestroy()
        {
            // stop and dispose of the timer when the service is destroyed
            StopCoroutine(FlushTimer());

            cancellationTokenSource.Cancel(); // Cancel the thread when the object is destroyed
            cancellationTokenSource.Dispose();
        }
#endregion

#region internal
        private IEnumerator FlushTimer() {
            while (true) {
                yield return new WaitForSeconds(AnalyticsConstants.CACHE_FLUSH_TIME_SECONDS);
                FlushCache();
            }
        }

        private async void FlushCache()
        {
            if(eventCache.Count > 0) {
                // copy the cache to avoid modifying it while iterating
                List<string> eventsToFlush = new List<string>(eventCache);

                // clear the cache
                eventCache.Clear();

                // send the batch of events for io
                var success = await SendEventBatch(eventsToFlush);
                
                // if the request failed, persist the payload to disk in a separate task
                if(!success) {
                    PersistEventBatchAsync(eventsToFlush);
                }
            }
        }

        private int CalculateCacheSizeInBytes()
        {
            // calculate the total size of the cached events in bytes
            return eventCache.Sum(e => e.Length);
        }

        private void BuildDeviceInfo()
        {
            deviceInfo = new Dictionary<string, object>
            {
                { AnalyticsConstants.PROP_DEVICE_NAME, SystemInfo.deviceName },
                { AnalyticsConstants.PROP_DEVICE_MODEL, SystemInfo.deviceModel },
                { AnalyticsConstants.PROP_DEVICE_TYPE, SystemInfo.deviceType },
                { AnalyticsConstants.PROP_DEVICE_UNIQUE_ID, SystemInfo.deviceUniqueIdentifier },
                { AnalyticsConstants.PROP_DEVICE_OS, SystemInfo.operatingSystem },
                { AnalyticsConstants.PROP_DEVICE_OS_FAMILY, SystemInfo.operatingSystemFamily },
                { AnalyticsConstants.PROP_DEVICE_CPU, SystemInfo.processorType }
            };
        }

        private Dictionary<string, object> BuildBaseEvent(string eventName, Dictionary<string, object> eventProps = null)
        {
            // handle null eventProps
            eventProps = eventProps ?? new Dictionary<string, object>();

            // create a dictionary to represent the event
            var evt = new Dictionary<string, object>
            {
                { AnalyticsConstants.PROP_NAME, eventName }, // event_name
                { AnalyticsConstants.PROP_ID, Guid.NewGuid().ToString("N") }, // event_id
                { AnalyticsConstants.PROP_VERSION, EventsVersion }, // event_version
                { AnalyticsConstants.PROP_TIME_LOCAL, TDKTimeKeeper.LocalEpochTimeInt64 }, // event_time_local
                { AnalyticsConstants.PROP_TIME_SERVER, TDKTimeKeeper.ServerEpochTimeInt64 }, // event_time_server
                { AnalyticsConstants.PROP_PROPERTIES, eventProps }, // event_properties
                { AnalyticsConstants.PROP_DEVICE, deviceInfo }
            };

            return evt;
        }
#endregion
    }
}
