#if TDK_HELIKA
using System.Collections.Generic;
using Helika;

namespace Treasure
{
    public class TDKHelikaService : TDKBaseService
    {
        private TDKHelikaConfig _config;
        private int _chainId = -1;

        public override void Awake()
        {
            base.Awake();

            _config = TDK.AppConfig.GetModuleConfig<TDKHelikaConfig>();

            if (_config != null) {
                EventManager.Instance.Init(
                    _config.ApiKey,
                    TDK.AppConfig.CartridgeTag,
                    TDK.AppConfig.Environment == TDKConfig.Env.PROD ? HelikaEnvironment.Production : HelikaEnvironment.Develop,
                    TelemetryLevel.All,
                    TDK.AppConfig.Environment == TDKConfig.Env.DEV
                );
            } else {
                TDKLogger.LogWarning("[TDKHelikaService] Helika config not found, skipping initialization.");
            }
        }

        public void SetTreasureConnectInfo(string smartWalletAddress, int chainId)
        {
            EventManager.Instance.SetPlayerID(smartWalletAddress);
            _chainId = chainId;
        }

        public void TrackEvent(string eventName, Dictionary<string, object> eventProps = null)
        {
            // helika doesn't handle null event props
            if (eventProps == null)
            {
                eventProps = new Dictionary<string, object>();
            }

            eventProps.Add(AnalyticsConstants.PROP_TDK_VERSION, TDKVersion.version);
            eventProps.Add(AnalyticsConstants.PROP_TDK_FLAVOUR, TDKVersion.name);
            eventProps.Add(AnalyticsConstants.PROP_APP_ENVIRONMENT, TDK.AppConfig.Environment);

            // add chain id (using indexer in case it was added upstream)
            eventProps[AnalyticsConstants.PROP_CHAIN_ID] = _chainId;

            eventProps.Add(AnalyticsConstants.PROP_TDK_VERSION, TDKVersion.version);
            eventProps.Add(AnalyticsConstants.PROP_TDK_FLAVOUR, TDKVersion.name);
            eventProps.Add(AnalyticsConstants.PROP_APP_ENVIRONMENT, TDK.AppConfig.Environment);

            // add chain id (using indexer in case it was added upstream)
            eventProps[AnalyticsConstants.PROP_CHAIN_ID] = _chainId;

            EventManager.Instance.SendEvent(eventName, eventProps);
        }
    }
}
#endif
