using System;
using System.Linq;
using Thirdweb;

namespace Treasure
{
    public class TDKThirdwebService : TDKBaseService
    {
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

            ChainId defaultChainId = TDK.AppConfig.DefaultChainId;
            
            if (defaultChainId != ChainId.Unknown) {
                InitializeSDK(Constants.ChainIdToName[defaultChainId]);
            } else {
                TDKLogger.LogWarning("[TDKThirdwebService] Invalid default chain in config, skipping initialization.");
            }            
        }

        public void InitializeSDK(string chainIdentifier)
        {
            TDKLogger.LogDebug("Initializing Thirdweb SDK for chain: " + chainIdentifier);
            var tdkConfig = TDK.AppConfig;
            var supportedChains = ((ChainId[])Enum.GetValues(typeof(ChainId)))
                .Where(chainId => chainId != ChainId.Unknown)
                .Select(chainId => new ThirdwebChainData { chainName = Constants.ChainIdToName[chainId] })
                .ToArray();

            // TODO this is copied code from ThirdwebManager.cs, we should refactor it so it wont get outdated
            var smartWalletConfig = new ThirdwebSDK.SmartWalletConfig()
            {
                factoryAddress = string.IsNullOrEmpty(tdkConfig.FactoryAddress) ? Thirdweb.AccountAbstraction.Constants.DEFAULT_FACTORY_ADDRESS : tdkConfig.FactoryAddress,
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
                clientId = tdkConfig.ClientId,
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
