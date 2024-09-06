using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Thirdweb;
using Thirdweb.Unity;
using Thirdweb.Unity.Helpers;

namespace Treasure
{
    public class TDKThirdwebService : TDKBaseService
    {
        // v5.0.0 --->>>
        public ThirdwebClient Client { get; private set; }
        public IThirdwebWallet ActiveWallet { get; private set; }
        public bool Initialized { get; private set;};
        
        private string BundleId { get; set; }
        private Dictionary<string, IThirdwebWallet> _walletMapping;
        // v5.0.0 <<<---

        // private ThirdwebSDK _sdk;

        // public ThirdwebSDK SDK
        // {
        //     get { return _sdk; }
        // }

        // public Wallet Wallet
        // {
        //     get { return _sdk.Wallet; }
        // }

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

            // v5.0.0 --->>>
            BundleId ??= Application.identifier ?? $"com.{Application.companyName}.{Application.productName}";

            Client = ThirdwebClient.Create(
                clientId: tdkConfig.ClientId,
                bundleId: BundleId,
                httpClient: Application.platform == RuntimePlatform.WebGLPlayer ? new UnityThirdwebHttpClient() : new ThirdwebHttpClient(),
                headers: new Dictionary<string, string>
                {
                    { "x-sdk-name", Application.platform == RuntimePlatform.WebGLPlayer ? "UnitySDK_WebGL" : "UnitySDK" },
                    { "x-sdk-os", Application.platform.ToString() },
                    { "x-sdk-platform", "unity" },
                    { "x-sdk-version", ThirdwebManager.THIRDWEB_UNITY_SDK_VERSION },
                    { "x-client-id", tdkConfig.ClientId },
                    { "x-bundle-id", BundleId }
                }
            );

            ThirdwebDebug.Log("ThirdwebManager initialized.");

            _walletMapping = new Dictionary<string, IThirdwebWallet>();

            Initialized = true;
            // v5.0.0 <<<---

            // var supportedChains = ((ChainId[])Enum.GetValues(typeof(ChainId)))
            //     .Where(chainId => chainId != ChainId.Unknown)
            //     .Select(chainId => new ThirdwebChainData { chainName = Constants.ChainIdToName[chainId] })
            //     .ToArray();

            // // TODO this is copied code from ThirdwebManager.cs, we should refactor it so it wont get outdated
            // var smartWalletConfig = new ThirdwebSDK.SmartWalletConfig()
            // {
            //     factoryAddress = string.IsNullOrEmpty(tdkConfig.FactoryAddress) ? Thirdweb.AccountAbstraction.Constants.DEFAULT_FACTORY_ADDRESS : tdkConfig.FactoryAddress,
            //     gasless = true,
            //     erc20PaymasterAddress = null,
            //     erc20TokenAddress = null,
            //     bundlerUrl = $"https://{chainIdentifier}.bundler.thirdweb.com",
            //     paymasterUrl = $"https://{chainIdentifier}.bundler.thirdweb.com",
            //     entryPointAddress = Thirdweb.AccountAbstraction.Constants.DEFAULT_ENTRYPOINT_ADDRESS,
            // };

            // var options = new ThirdwebSDK.Options
            // {
            //     smartWalletConfig = smartWalletConfig,
            //     clientId = tdkConfig.ClientId,
            //     supportedChains = supportedChains
            // };

            // _sdk = new ThirdwebSDK(
            //     chainIdentifier,
            //     (int)Constants.NameToChainId[chainIdentifier],
            //     options
            // );
        }
    }
}
