#if TDK_THIRDWEB
using Thirdweb;
using UnityEngine;

namespace Treasure
{
    public class TDKThirdwebService : TDKBaseService
    {
        private TDKThirdwebConfig _config;

        private ThirdwebSDK _thirdwebSDK;

        public ThirdwebSDK ThirdwebSDK
        {
            get { return _thirdwebSDK; }
        }

        public override void Awake()
        {
            base.Awake();

            _config = TDK.Instance.AppConfig.GetModuleConfig<TDKThirdwebConfig>();

            InitializeThirdwebSDK(_config.DefaultChainIdentifier);
        }

        public void InitializeThirdwebSDK(string chainIdentifier)
        {
            var _smartWalletConfig = new ThirdwebSDK.SmartWalletConfig
            {
                factoryAddress = _config.FactoryAddress,
                gasless = true
            };

            var _supportedChains = new ThirdwebChainData[] {
                new ThirdwebChainData { chainName = "arbitrum"},
                new ThirdwebChainData { chainName = "arbitrum-sepolia"},
                new ThirdwebChainData { chainName = "ethereum"},
                new ThirdwebChainData { chainName = "sepolia"},
                new ThirdwebChainData { chainName = "treasure-ruby"}
            };

            var _options = new ThirdwebSDK.Options
            {
                smartWalletConfig = _smartWalletConfig,
                clientId = _config.ClientId,
                supportedChains = _supportedChains
            };

            _thirdwebSDK = new ThirdwebSDK(
                chainIdentifier,
                (int) Constants.NameToChainId[chainIdentifier],
                _options
            );
        }

        public Wallet Wallet
        {
            get { return _thirdwebSDK.Wallet; }
        }
    }
}
#endif
