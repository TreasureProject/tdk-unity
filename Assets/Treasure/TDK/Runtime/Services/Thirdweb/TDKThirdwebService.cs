using UnityEngine;
using Thirdweb;
using Thirdweb.Unity;
using Thirdweb.Unity.Helpers;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.Numerics;
using WalletConnectUnity.Modal;
using WalletConnectUnity.Core;
using Cysharp.Threading.Tasks;

namespace Treasure
{
    public class TDKThirdwebService : TDKBaseService
    {
        public ThirdwebClient Client { get; private set; }
        public SmartWallet ActiveWallet { get; private set; }
        public bool Initialized { get; private set; }

        private string bundleId;
        private CancellationTokenSource _connectionCancelationTokenSource;

        private bool _didInitWalletConnect = false;

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

            bundleId = !string.IsNullOrEmpty(Application.identifier) ? Application.identifier : $"com.{Application.companyName}.{Application.productName}";

            Client = ThirdwebClient.Create(
                clientId: clientId,
                bundleId: bundleId.ToLower(),
                httpClient: Application.platform == RuntimePlatform.WebGLPlayer
                    ? new UnityThirdwebHttpClient()
                    : new ThirdwebHttpClient(),
                sdkName: Application.platform == RuntimePlatform.WebGLPlayer ? "UnitySDK_WebGL" : "UnitySDK",
                sdkOs: Application.platform.ToString(),
                sdkPlatform: "unity",
                sdkVersion: ThirdwebManagerBase.THIRDWEB_UNITY_SDK_VERSION
            );

            TDKLogger.LogInfo("TDKThirdwebService initialized.");

            Initialized = true;
        }

        public async Task ConnectWallet(EcosystemWalletOptions ecosystemWalletOptions, int chainId, bool isSilentReconnect, string twAuthTokenOverride = null)
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
                    siweSigner: ecosystemWalletOptions.SiweSigner,
                    storageDirectoryPath: ecosystemWalletOptions.StorageDirectoryPath,
                    twAuthTokenOverride: twAuthTokenOverride
                );
                cancellationToken.ThrowIfCancellationRequested();
                
                var isEmailLogin = ecosystemWalletOptions.AuthProvider == AuthProvider.Default;
                var isSocialsLogin = Enum.IsDefined(typeof(SocialAuthProvider), (int) ecosystemWalletOptions.AuthProvider);
                var isWalletLogin = ecosystemWalletOptions.AuthProvider == AuthProvider.Siwe;

                var isConnected = await ecosystemWallet.IsConnected();
                if (!isConnected)
                {
                    if (isSilentReconnect)
                    {
                        TDKLogger.LogDebug($"Could not recreate user automatically, skipping silent reconnect");
                        return;
                    }

                    TDKLogger.LogDebug("Session does not exist or is expired, proceeding with EcosystemWallet authentication.");

                    
                    if (isEmailLogin)
                    {
                        await ecosystemWallet.SendOTP();
                        cancellationToken.ThrowIfCancellationRequested();
                        var otpModal = TDKConnectUIManager.Instance.ShowOtpModal(ecosystemWalletOptions.Email);
                        _ = await otpModal.LoginWithOtp(ecosystemWallet);
                    }
                    else if (isSocialsLogin)
                    {
                        _ = await ecosystemWallet.LoginWithOauth(
                            isMobile: Application.isMobilePlatform,
                            browserOpenAction: (url) => Application.OpenURL(url),
                            mobileRedirectScheme: bundleId + "://",
                            browser: new CrossPlatformUnityBrowser(),
                            cancellationToken: cancellationToken
                        );
                    }
                    else if (isWalletLogin)
                    {
                        TDKConnectUIManager.Instance.GetTransitionModal().SetInfoLabels(
                            "Connecting...",
                            "Sign transaction in your external wallet"
                        );
                        _ = await ecosystemWallet.LoginWithSiwe(TDK.Connect.ChainIdNumber);
                    }
                    else
                    {
                        throw new Exception("Unexpected AuthProvider value");
                    }
                }
                cancellationToken.ThrowIfCancellationRequested();
                if (isWalletLogin)
                {
                    TDKConnectUIManager.Instance.GetTransitionModal().SetInfoLabels(
                        "Almost there...",
                        "Upgrading to smart wallet"
                    );
                }
                smartWallet = await SmartWallet.Create(
                    personalWallet: ecosystemWallet,
                    chainId: chainId,
                    factoryAddress: Constants.MANAGED_ACCOUNT_FACTORY_ADDRESS,
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

        public async Task ConnectExternalWallet(int chainId)
        {
            _connectionCancelationTokenSource?.Cancel();
            var supportedChains = new BigInteger[] { chainId };
            WalletConnectWallet wallet = await WalletConnectWallet.Create(
                client: Client,
                initialChainId: chainId,
                supportedChains: supportedChains,
                includedWalletIds: null
            );
            var options = new EcosystemWalletOptions(authprovider: AuthProvider.Siwe, siweSigner: wallet);
            await ConnectWallet(options, TDK.Connect.ChainIdNumber, isSilentReconnect: false);
        }

        public void EnsureWalletConnectInitialized()
        {
            if (_didInitWalletConnect == false)
            {
                _didInitWalletConnect = true;
                TDKMainThreadDispatcher.Enqueue(async () => {
                    // initialize modal only after user clicks button to prevent unnecessary work when not using this flow
                    TDKLogger.LogDebug("Initializing WalletConnectModal...");
                    await WalletConnectModal.InitializeAsync();
                    TDKLogger.LogDebug("WalletConnectModal initialized!");
                    // disconnect any previous session, because:
                    // - thirdweb sdk does this internally anyways as soon as WalletConnectWallet is created (due to instability)
                    // - unexpected errors and freezes happen when chain is switched in metamask and the session was recovered
                    if (WalletConnect.Instance.IsConnected)
                    {
                        TDKLogger.LogDebug("Disconnecting previous WalletConnect session...");
                        await WalletConnect.Instance.DisconnectAsync();
                        TDKLogger.LogDebug("WalletConnect disconnected!");
                    }
                });
            }
        }

        public async Task<bool> WaitForWalletConnectReady(float maxWait)
        {
            float timeout = maxWait;
            await new WaitUntil(() => {
                timeout -= Time.deltaTime;
                return timeout <= 0 || WalletConnectModal.IsReady;
            });
            return WalletConnectModal.IsReady;
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
                await UniTask.WaitForEndOfFrame(this);
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

        public Task<bool> IsZkSyncChain(int chainId)
        {
            return Utils.IsZkSync(Client, new BigInteger(chainId));
        }

        public async Task<Transaction> WriteTransaction(WriteTransactionBody body)
        {
            if (!await IsWalletConnected())
            {
                throw new UnityException("[TDKThirdwebService.WriteTransaction] No active wallet connected");
            }
            var transaction = await PrepareTransactionFromBody(body);
            var receipt = await ThirdwebTransaction.SendAndWaitForTransactionReceipt(transaction);
            return ParseThirdwebTransactionReceipt(receipt);
        }

        public async Task<Transaction> WriteTransactionRaw(SendRawTransactionBody body)
        {
            if (!await IsWalletConnected())
            {
                throw new UnityException("[TDKThirdwebService.WriteTransactionRaw] No active wallet connected");
            }
            var transaction = await PrepareTransactionFromBody(body);
            var receipt = await ThirdwebTransaction.SendAndWaitForTransactionReceipt(transaction);
            return ParseThirdwebTransactionReceipt(receipt);
        }

        public async Task<ThirdwebTransaction> PrepareTransactionFromBody(WriteTransactionBody body)
        {
            var contract = await ThirdwebContract.Create(Client, body.address, TDK.Connect.ChainIdNumber, body.abi);
            var transaction = await ThirdwebContract.Prepare(ActiveWallet, contract, body.functionName, 0, parameters: body.args);
            SetTxOverrides(transaction,
                value: body.txOverrides.value,
                gas: body.txOverrides.gas,
                maxFeePerGas: body.txOverrides.maxFeePerGas,
                maxPriorityFeePerGas: body.txOverrides.maxPriorityFeePerGas
            );
            return transaction;
        }

        public async Task<ThirdwebTransaction> PrepareTransactionFromBody(SendRawTransactionBody body)
        {
            var transactionInput = new ThirdwebTransactionInput(
                chainId: TDK.Connect.ChainIdNumber,
                to: body.to,
                value: string.IsNullOrEmpty(body.value) ? 0 : BigInteger.Parse(body.value),
                data: body.data
            );
            var transaction = await ThirdwebTransaction.Create(ActiveWallet, transactionInput);
            SetTxOverrides(transaction,
                gas: body.txOverrides.gas,
                maxFeePerGas: body.txOverrides.maxFeePerGas,
                maxPriorityFeePerGas: body.txOverrides.maxPriorityFeePerGas
            );
            return transaction;
        }

        private void SetTxOverrides(
            ThirdwebTransaction transaction,
            string value = null,
            string gas = null,
            string maxFeePerGas = null,
            string maxPriorityFeePerGas = null
        )
        {
            if (!string.IsNullOrEmpty(value))
            {
                transaction.SetValue(BigInteger.Parse(value));
            }
            if (!string.IsNullOrEmpty(gas))
            {
                transaction.SetGasLimit(BigInteger.Parse(gas));
            }
            if (!string.IsNullOrEmpty(maxFeePerGas))
            {
                transaction.SetMaxFeePerGas(BigInteger.Parse(maxFeePerGas));
            }
            if (!string.IsNullOrEmpty(maxPriorityFeePerGas))
            {
                transaction.SetMaxPriorityFeePerGas(BigInteger.Parse(maxPriorityFeePerGas));
            }
        }

        private Transaction ParseThirdwebTransactionReceipt(ThirdwebTransactionReceipt receipt)
        {
            var status = receipt.Status.ToString();
            const string revertedStatus = "0";
            const string successStatus = "1";
            if (status == revertedStatus)
            {
                throw new UnityException($"[ParseThirdwebTransactionReceipt] Transaction reverted ({receipt.TransactionHash})");
            }
            if (status != successStatus)
            {
                throw new UnityException($"[ParseThirdwebTransactionReceipt] Transaction errored ({receipt.TransactionHash})");
            }
            return new Transaction {
                status = "mined",
                transactionHash = receipt.TransactionHash,
                errorMessage = null,
            };
        }

        public async void TrackThirdwebAnalytics(string source, string action, string walletType, string walletAddress)
        {
            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(action) || string.IsNullOrEmpty(walletType) || string.IsNullOrEmpty(walletAddress))
            {
                TDKLogger.LogWarning("[TDKThirdwebService.TrackUsage] Invalid usage analytics parameters");
                return;
            }

            try
            {
                var content = new System.Net.Http.StringContent(
                    Newtonsoft.Json.JsonConvert.SerializeObject(
                        new
                        {
                            source,
                            action,
                            walletAddress,
                            walletType,
                        }
                    ),
                    System.Text.Encoding.UTF8,
                    "application/json"
                );
                _ = await Client.HttpClient.PostAsync("https://c.thirdweb.com/event", content);
            }
            catch
            {
                TDKLogger.LogWarning($"[TDKThirdwebService.TrackUsage] Failed to report usage analytics");
            }
        }
    }
}
