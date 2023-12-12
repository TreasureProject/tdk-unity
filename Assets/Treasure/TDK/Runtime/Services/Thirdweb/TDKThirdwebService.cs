namespace Treasure
{
    public class TDKThirdwebService : TDKBaseService
    {
        private TDKThirdwebConfig _config;

        public override void Awake()
        {
            base.Awake();

            _config = TDK.Instance.AppConfig.GetModuleConfig<TDKThirdwebConfig>(); 

            // ThirdwebSDK sdk = new ThirdwebSDK("arbitrum-seploia", {
            //     clientId = myClientId // you can get client id from dashboard settings
            // });
        }
    }
}
