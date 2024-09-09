using System.Collections.Generic;
using UnityEngine;
using Thirdweb;
using Thirdweb.Unity;
using Thirdweb.Unity.Helpers;
using System.Threading.Tasks;
using System;

namespace Treasure
{
    public class TDKThirdwebService : TDKBaseService
    {
        public ThirdwebClient Client { get; private set; }
        public SmartWallet ActiveWallet { get; private set; }
        public bool Initialized { get; private set; }
        
        private string bundleId;
        private Dictionary<string, IThirdwebWallet> _walletMapping;

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

        // TODO make this private if possible
        public void InitializeSDK(string chainIdentifier)
        {
            TDKLogger.LogDebug("Initializing Thirdweb SDK for chain: " + chainIdentifier);

            var clientId = TDK.AppConfig.ClientId;

            bundleId ??= Application.identifier ?? $"com.{Application.companyName}.{Application.productName}";

            Client = ThirdwebClient.Create(
                clientId: clientId,
                bundleId: bundleId,
                httpClient: Application.platform == RuntimePlatform.WebGLPlayer ? new UnityThirdwebHttpClient() : new ThirdwebHttpClient(),
                headers: new Dictionary<string, string>
                {
                    { "x-sdk-name", Application.platform == RuntimePlatform.WebGLPlayer ? "UnitySDK_WebGL" : "UnitySDK" },
                    { "x-sdk-os", Application.platform.ToString() },
                    { "x-sdk-platform", "unity" },
                    { "x-sdk-version", ThirdwebManager.THIRDWEB_UNITY_SDK_VERSION },
                    { "x-client-id", clientId },
                    { "x-bundle-id", bundleId }
                }
            );

            ThirdwebDebug.Log("ThirdwebManager initialized.");

            _walletMapping = new Dictionary<string, IThirdwebWallet>();

            Initialized = true;
        }
        
        public async Task ConnectWallet(InAppWalletOptions inAppWalletOptions, int chainId) {
            var thirdwebService = TDKServiceLocator.GetService<TDKThirdwebService>();
            var inAppWallet = await InAppWallet.Create(
                client: thirdwebService.Client,
                email: inAppWalletOptions.Email,
                phoneNumber: inAppWalletOptions.PhoneNumber,
                authProvider: inAppWalletOptions.AuthProvider,
                storageDirectoryPath: inAppWalletOptions.StorageDirectoryPath
            );
            if (!await inAppWallet.IsConnected()) {
                ThirdwebDebug.Log("Session does not exist or is expired, proceeding with InAppWallet authentication.");

                if (inAppWalletOptions.AuthProvider == AuthProvider.Default)
                {
                    await inAppWallet.SendOTP();
                    var otpModal = TDKConnectUIManager.Instance.ShowOtpModal(inAppWalletOptions.Email);
                    _ = await otpModal.LoginWithOtp(inAppWallet);
                }
                else
                {
                    _ = await inAppWallet.LoginWithOauth(
                        isMobile: Application.isMobilePlatform,
                        browserOpenAction: (url) => Application.OpenURL(url),
                        mobileRedirectScheme: bundleId + "://",
                        browser: new CrossPlatformUnityBrowser()
                    );
                }
            }
            var smartWallet = await SmartWallet.Create(
                personalWallet: inAppWallet,
                chainId: chainId,
                gasless: true,
                factoryAddress: TDK.AppConfig.FactoryAddress
            );

            ActiveWallet = smartWallet;
            // TODO add to _walletMapping?
        }

        public async Task<bool> IsWalletConnected()
        {
            return ActiveWallet != null && await ActiveWallet.IsConnected();
        }

        public async Task DisconnectWallets(bool endSession = false)
        {
            // TODO check if endSession is still needed
            // TODO check _walletMapping?
            if (ActiveWallet != null)
            {
                // TODO InAppWalletUI.Instance.Cancel(); // cancel any in progress connect operations
                if (await ActiveWallet.IsConnected()) {
                    var personalWallet = await ActiveWallet.GetPersonalWallet();
                    await personalWallet.Disconnect();
                    await ActiveWallet.Disconnect();
                }
                await new WaitForEndOfFrame();
                ActiveWallet = null;
            }
        }

        public async Task SwitchNetwork(int chainId)
        {
            if (ActiveWallet != null && await ActiveWallet.IsConnected()) {
                await ActiveWallet.SwitchNetwork(chainId);
            }
        }
    }
}
