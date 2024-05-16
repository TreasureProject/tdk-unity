#if TDK_THIRDWEB
using Thirdweb;
using UnityEngine;

namespace Treasure
{
    public class TDKThirdwebService : TDKBaseService
    {
        private TDKThirdwebConfig _config;

        public override async void Awake()
        {
            base.Awake();

            _config = TDK.Instance.AppConfig.GetModuleConfig<TDKThirdwebConfig>();

            // Wait for the ThirdwebManager Awake method to run and instantiate the shared instance
            await new WaitUntil(() => ThirdwebManager.Instance != null);

            TDKLogger.Log($"Initializing ThirdwebManager with config");
            ThirdwebManager.Instance.clientId = _config.ClientId;
            ThirdwebManager.Instance.factoryAddress = _config.FactoryAddress;
            ThirdwebManager.Instance.gasless = true;
            ThirdwebManager.Instance.Initialize(_config.DefaultChainIdentifier);
        }

        public Wallet Wallet
        {
            get { return ThirdwebManager.Instance.SDK.Wallet; }
        }
    }
}
#endif
