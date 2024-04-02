using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Treasure
{
    public partial class TDKAnalyticsService : TDKBaseService
    {
        public const uint EventsVersion = 1;

        private Dictionary<string, object> _deviceInfo;
        private Dictionary<string, object> _appInfo;
        private string _sessionId;
        private List<string> _eventCache = new List<string>();
        private string _smartAccountAddress;
        private int _chainId = -1;

#region lifecycle
        public override void Awake()
        {
            base.Awake();

            StartNewSession();
            BuildDeviceInfo();
            BuildAppInfo();
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

        void OnApplicationQuit()
        {
            FlushCache();
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
            if(_eventCache.Count > 0) {
                // copy the cache to avoid modifying it while iterating
                List<string> eventsToFlush = new List<string>(_eventCache);

                // clear the cache
                _eventCache.Clear();

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
            return _eventCache.Sum(e => e.Length);
        }

        private void BuildDeviceInfo()
        {
            _deviceInfo = new Dictionary<string, object>
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

        private void BuildAppInfo()
        {
            _appInfo = new Dictionary<string, object>
            {
                { AnalyticsConstants.PROP_APP_IDENTIFIER, Application.identifier },
                { AnalyticsConstants.PROP_APP_IS_EDITOR, Application.isEditor },
                { AnalyticsConstants.PROP_APP_VERSION, Application.version },
                { AnalyticsConstants.PROP_APP_ENVIRONMENT, TDK.Instance.AppConfig.Environment }
            };
        }

        private Dictionary<string, object> BuildBaseEvent(string eventName, Dictionary<string, object> eventProps = null)
        {
            // handle null eventProps
            eventProps = eventProps ?? new Dictionary<string, object>();

            // create a dictionary to represent the event
            var evt = new Dictionary<string, object>
            {
                { AnalyticsConstants.PROP_SMART_ACCOUNT, string.IsNullOrEmpty(_smartAccountAddress) ? string.Empty : _smartAccountAddress }, // smart_account
                { AnalyticsConstants.PROP_CHAIN_ID, _chainId }, // chain_id
                { AnalyticsConstants.CARTRIDGE_TAG, TDK.Instance.AppConfig.CartridgeTag }, // cartridgeTag
                { AnalyticsConstants.PROP_NAME, eventName }, // event_name
                { AnalyticsConstants.PROP_SESSION_ID, _sessionId }, // session_id
                { AnalyticsConstants.PROP_ID, Guid.NewGuid().ToString("N") }, // event_id
                { AnalyticsConstants.PROP_VERSION, EventsVersion }, // event_version
                { AnalyticsConstants.PROP_TIME_LOCAL, TDKTimeKeeper.LocalEpochTime }, // event_time_local
                { AnalyticsConstants.PROP_TIME_SERVER, TDKTimeKeeper.ServerEpochTime }, // event_time_server
                { AnalyticsConstants.PROP_PROPERTIES, eventProps }, // event_properties
                { AnalyticsConstants.PROP_DEVICE, _deviceInfo },
                { AnalyticsConstants.PROP_APP, _appInfo }
            };

            return evt;
        }
#endregion

#region internal
        public void SetTreasureConnectInfo(string smartWalletAddress, int chainId)
        {
            _smartAccountAddress = smartWalletAddress;
            _chainId = chainId;
        }
#endregion
    }
}
