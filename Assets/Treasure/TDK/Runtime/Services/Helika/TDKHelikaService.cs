#if TDK_HELIKA
using System.Collections.Generic;
using Helika;

namespace Treasure
{
    public class TDKHelikaService : TDKBaseService
    {
        private TDKHelikaConfig _config;

        public override void Awake()
        {
            base.Awake();

            _config = TDK.Instance.AppConfig.GetModuleConfig<TDKHelikaConfig>();

            EventManager.Instance.Init(
                _config.ApiKey,
                TDK.Instance.AppConfig.CartridgeTag,
                TDK.Instance.AppConfig.Environment == TDKConfig.Env.PROD ? HelikaEnvironment.Production : HelikaEnvironment.Develop,
                TelemetryLevel.All,
                TDK.Instance.AppConfig.Environment == TDKConfig.Env.DEV
            );
        }

        public void SetPlayerId(string playerId)
        {
            EventManager.Instance.SetPlayerID(playerId);
        }

        public void TrackEvent(string eventName, Dictionary<string, object> eventProps = null)
        {
            // helika doesn't handel null event props
            if(eventProps == null) { eventProps = new Dictionary<string, object>();}

            EventManager.Instance.SendEvent(eventName, eventProps);
        }
    }
}
#endif
