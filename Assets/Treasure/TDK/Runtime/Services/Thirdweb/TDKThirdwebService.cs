using UnityEngine;
using Thirdweb;
using Thirdweb.Unity;
using Thirdweb.Unity.Helpers;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace Treasure
{
    public class TDKThirdwebService : TDKBaseService
    {
        public ThirdwebClient Client { get; private set; }
        public SmartWallet ActiveWallet { get; private set; }
        public bool Initialized { get; private set; }

        private string bundleId;
        private CancellationTokenSource _connectionCancelationTokenSource;

        public override void Awake()
        {
            base.Awake();

            ChainId defaultChainId = TDK.AppConfig.DefaultChainId;

            if (defaultChainId != ChainId.Unknown)
            {
                InitializeSDK(Constants.ChainIdToName[defaultChainId]);
            }
            else
            {
                TDKLogger.LogWarning("[TDKThirdwebService] Invalid default chain in config, skipping initialization.");
            }
        }

        private void InitializeSDK(string chainIdentifier)
        {
            TDKLogger.LogDebug("Initializing Thirdweb SDK for chain: " + chainIdentifier);

            var clientId = TDK.AppConfig.ClientId;

            bundleId ??= Application.identifier ?? $"com.{Application.companyName}.{Application.productName}";

            Client = ThirdwebClient.Create(
                clientId: clientId,
                bundleId: bundleId.ToLower(),
                httpClient: Application.platform == RuntimePlatform.WebGLPlayer ? new UnityThirdwebHttpClient() : new ThirdwebHttpClient()
            );

            TDKLogger.LogInfo("TDKThirdwebService initialized.");

            Initialized = true;
        }

        public async Task ConnectWallet(EcosystemWalletOptions ecosystemWalletOptions, int chainId, bool isSilentReconnect)
        {
            // Allow only one attempt to connect at a time, a new one will cancel any previous
            _connectionCancelationTokenSource?.Cancel();
            _connectionCancelationTokenSource = new CancellationTokenSource();
            var cancellationToken = _connectionCancelationTokenSource.Token;

            EcosystemWallet ecosystemWallet = null;
            SmartWallet smartWallet = null;
            try
            {
                ecosystemWallet = await EcosystemWallet.Create(
                    ecosystemId: TDK.AppConfig.EcosystemId,
                    ecosystemPartnerId: TDK.AppConfig.EcosystemPartnerId,
                    client: Client,
                    email: ecosystemWalletOptions.Email,
                    phoneNumber: ecosystemWalletOptions.PhoneNumber,
                    authProvider: ecosystemWalletOptions.AuthProvider,
                    storageDirectoryPath: ecosystemWalletOptions.StorageDirectoryPath
                );
                cancellationToken.ThrowIfCancellationRequested();
                var isConnected = await ecosystemWallet.IsConnected();
                if (!isConnected)
                {
                    if (isSilentReconnect)
                    {
                        TDKLogger.LogDebug($"Could not recreate user automatically, skipping silent reconnect");
                        return;
                    }

                    TDKLogger.LogDebug("Session does not exist or is expired, proceeding with EcosystemWallet authentication.");

                    if (ecosystemWalletOptions.AuthProvider == AuthProvider.Default)
                    {
                        await ecosystemWallet.SendOTP();
                        cancellationToken.ThrowIfCancellationRequested();
                        var otpModal = TDKConnectUIManager.Instance.ShowOtpModal(ecosystemWalletOptions.Email);
                        _ = await otpModal.LoginWithOtp(ecosystemWallet);
                    }
                    else
                    {
                        _ = await ecosystemWallet.LoginWithOauth(
                            isMobile: Application.isMobilePlatform,
                            browserOpenAction: (url) => Application.OpenURL(url),
                            mobileRedirectScheme: bundleId + "://",
                            browser: new CrossPlatformUnityBrowser(),
                            cancellationToken: cancellationToken
                        );
                    }
                }
                cancellationToken.ThrowIfCancellationRequested();
                smartWallet = await SmartWallet.Create(
                    personalWallet: ecosystemWallet,
                    chainId: chainId,
                    factoryAddress: TDK.Common.GetContractAddress(Contract.ManagedAccountFactory, (ChainId)chainId),
                    gasless: true
                );
                cancellationToken.ThrowIfCancellationRequested();

                TDKLogger.LogDebug("[TDKThirdwebService:ConnectWallet] Smart wallet successfully connected!");

                ActiveWallet = smartWallet;
            }
            catch (Exception ex)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    TDKLogger.LogInfo("[TDKThirdwebService:ConnectWallet] New connection attempt has been made, ignoring previous connection...");
                    TDKLogger.LogException("Wallet connection cancelled", ex);
                    throw new Exception("New connection attempt has been made");
                }
                throw;
            }
        }

        public async Task<bool> IsWalletConnected()
        {
            return ActiveWallet != null && await ActiveWallet.IsConnected();
        }

        public async Task DisconnectWallet()
        {
            _connectionCancelationTokenSource?.Cancel(); // cancel any in progress connect operations
            if (ActiveWallet != null)
            {
                TDKLogger.LogInfo("[TDKThirdwebService:DisconnectWallet] Clearing active wallet");
                if (await ActiveWallet.IsConnected())
                {
                    var personalWallet = await ActiveWallet.GetPersonalWallet();
                    await personalWallet.Disconnect();
                    await ActiveWallet.Disconnect();
                    TDKLogger.LogInfo("[TDKThirdwebService:DisconnectWallet] Active wallet disconnected");
                }
                await new WaitForEndOfFrame();
                ActiveWallet = null;
            }
        }

        public async Task SwitchNetwork(int chainId)
        {
            if (ActiveWallet != null && await ActiveWallet.IsConnected())
            {
                await ActiveWallet.SwitchNetwork(chainId);
            }
        }
    }
}
