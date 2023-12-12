using System.Collections.Generic;
using Helika;
using Newtonsoft.Json.Linq;

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
                _config.GameId, 
                HelikaEnvironment.Develop, //TODO abscract this
                true);
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
