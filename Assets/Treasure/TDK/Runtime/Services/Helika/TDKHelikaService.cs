#if TDK_HELIKA
using System.Collections.Generic;
using Helika;

namespace Treasure
{
    public class TDKHelikaService : TDKBaseService
    {
        private TDKHelikaConfig _config;

        public override async void Awake()
        {
            base.Awake();

            _config = TDK.Instance.AppConfig.GetModuleConfig<TDKHelikaConfig>();

            await EventManager.Instance.Init(
                _config.ApiKey,
                TDK.Instance.AppConfig.CartridgeTag,
                TDK.Instance.AppConfig.Environment == TDKConfig.Env.PROD ? HelikaEnvironment.Production : HelikaEnvironment.Develop,
                true
            );
        }

        public void SetPlayerId(string playerId)
        {
            EventManager.Instance.SetPlayerID(playerId);
        }

        public async void TrackEvent(string eventName, Dictionary<string, object> eventProps = null)
        {
            await EventManager.Instance.SendEvent(eventName, eventProps);
        }
    }
}
#endif
