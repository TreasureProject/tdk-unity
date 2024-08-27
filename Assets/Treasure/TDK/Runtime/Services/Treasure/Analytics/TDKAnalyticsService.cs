using UnityEngine;
using System;
using System.Collections.Generic;

namespace Treasure
{
    public partial class TDKAnalyticsService : TDKBaseService
    {
        private Dictionary<string, object> _deviceInfo;
        private Dictionary<string, object> _appInfo;
        private string _sessionId;
        private string _smartAccountAddress;
        private int _chainId = -1;

#region lifecycle
        public override void Awake()
        {
            base.Awake();

            StartNewSession();
            BuildDeviceInfo();
            BuildAppInfo();
#if !UNITY_WEBGL
            InitEventCaching();
#endif
        }

#if !UNITY_WEBGL
        void OnApplicationQuit()
        {
            TerminateCacheFlushing();
        }
#endif

#endregion

#region internal
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
                { AnalyticsConstants.PROP_APP_ENVIRONMENT, TDK.AppConfig.Environment }
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
                { AnalyticsConstants.CARTRIDGE_TAG, TDK.AppConfig.CartridgeTag }, // cartridgeTag
                { AnalyticsConstants.PROP_NAME, eventName }, // event_name
                { AnalyticsConstants.PROP_SESSION_ID, _sessionId }, // session_id
                { AnalyticsConstants.PROP_ID, Guid.NewGuid().ToString("N") }, // event_id
                { AnalyticsConstants.PROP_TDK_VERSION, TDKVersion.version }, // tdk_version
                { AnalyticsConstants.PROP_TDK_FLAVOUR, TDKVersion.name }, // tdk_flavour
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
