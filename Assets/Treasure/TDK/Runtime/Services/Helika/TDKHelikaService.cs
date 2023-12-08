namespace Treasure
{
    public class TDKHelikaService : TDKBaseService
    {
        private TDKHelikaConfig _config;

        public override void Awake()
        {
            base.Awake();

            _config = TDK.Instance.AppConfig.GetModuleConfig<TDKHelikaConfig>();
        }
    }
}
