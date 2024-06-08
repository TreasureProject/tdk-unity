#if TDK_THIRDWEB
using System;
using System.Linq;
using Thirdweb;

namespace Treasure
{
    public class TDKThirdwebService : TDKBaseService
    {
        private TDKThirdwebConfig _config;
        private ThirdwebSDK _sdk;

        public ThirdwebSDK SDK
        {
            get { return _sdk; }
        }

        public Wallet Wallet
        {
            get { return _sdk.Wallet; }
        }

        public override void Awake()
        {
            base.Awake();

            _config = TDK.Instance.AppConfig.GetModuleConfig<TDKThirdwebConfig>();

            InitializeSDK(_config.DefaultChainIdentifier);
        }

        public void InitializeSDK(string chainIdentifier)
        {
            var supportedChains = ((ChainId[])Enum.GetValues(typeof(ChainId)))
                .Where(chainId => chainId != ChainId.Unknown)
                .Select(chainId => new ThirdwebChainData { chainName = Constants.ChainIdToName[chainId] })
                .ToArray();

            var smartWalletConfig = new ThirdwebSDK.SmartWalletConfig
            {
                factoryAddress = _config.FactoryAddress,
                gasless = true
            };

            var options = new ThirdwebSDK.Options
            {
                smartWalletConfig = smartWalletConfig,
                clientId = _config.ClientId,
                supportedChains = supportedChains
            };

            _sdk = new ThirdwebSDK(
                chainIdentifier,
                (int)Constants.NameToChainId[chainIdentifier],
                options
            );
        }
    }
}
#endif
