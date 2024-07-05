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
            
            if (_config != null) {
                InitializeSDK(_config.DefaultChainIdentifier);
            } else {
                TDKLogger.LogWarning("[TDKThirdwebService] Thirdweb config not found, skipping initialization.");
            }            
        }

        public void InitializeSDK(string chainIdentifier)
        {
            var supportedChains = ((ChainId[])Enum.GetValues(typeof(ChainId)))
                .Where(chainId => chainId != ChainId.Unknown)
                .Select(chainId => new ThirdwebChainData { chainName = Constants.ChainIdToName[chainId] })
                .ToArray();

            // TODO this is copied code from ThirdwebManager.cs, we should refactor it so it wont get outdated
            var smartWalletConfig = new ThirdwebSDK.SmartWalletConfig()
            {
                factoryAddress = string.IsNullOrEmpty(_config.FactoryAddress) ? Thirdweb.AccountAbstraction.Constants.DEFAULT_FACTORY_ADDRESS : _config.FactoryAddress,
                gasless = true,
                erc20PaymasterAddress = null,
                erc20TokenAddress = null,
                bundlerUrl = $"https://{chainIdentifier}.bundler.thirdweb.com",
                paymasterUrl = $"https://{chainIdentifier}.bundler.thirdweb.com",
                entryPointAddress = Thirdweb.AccountAbstraction.Constants.DEFAULT_ENTRYPOINT_ADDRESS,
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
