#if TDK_THIRDWEB
using UnityEngine;
using UnityEngine.Events;
using Unity.Android.Types;
using System;
using System.Threading.Tasks;
using Thirdweb;

using System.Numerics;

namespace Treasure
{
    public class TDKThirdwebService : TDKBaseService
    {
        private TDKThirdwebConfig _config;
        
        private bool useSmartWallets = true;
        private ChainData _currentChainData;
        private string _address;
        private string _email;

        public UnityEvent<WalletConnection> onConnectionRequested = new UnityEvent<WalletConnection>();
        public UnityEvent<Exception> onConnectionError = new UnityEvent<Exception>();
        public UnityEvent<string> onConnected = new UnityEvent<string>();

        public Wallet Wallet
        {
            get { return ThirdwebManager.Instance.SDK.wallet; }
        }

        public override void Awake()
        {
            base.Awake();

            _config = TDK.Instance.AppConfig.GetModuleConfig<TDKThirdwebConfig>();
        }

        public void Start()
        {
            _currentChainData = ThirdwebManager.Instance.supportedChains.Find(x => x.identifier == ThirdwebManager.Instance.activeChain);
        }

        public async Task<string> Sign(string message)
        {
            return await ThirdwebManager.Instance.SDK.wallet.Sign(message);
        }

        public async Task<bool> ConnectEmail(string email)
        {
            _email = email;
            var wc = useSmartWallets
                ? new WalletConnection(
                    provider: WalletProvider.SmartWallet,
                    chainId: BigInteger.Parse(_currentChainData.chainId),
                    email: email,
                    authOptions: new AuthOptions(AuthProvider.EmailOTP),
                    personalWallet: WalletProvider.EmbeddedWallet
                )
                : new WalletConnection(
                    provider: WalletProvider.EmbeddedWallet,
                    chainId: BigInteger.Parse(_currentChainData.chainId),
                    email: email,
                    authOptions: new AuthOptions(AuthProvider.EmailOTP)
                );
           return await Connect(wc);
        }

        public string GetWalletAddress()
        {
            return _address;
        }

        public string GetUserEmail()
        {
            return _email;
        }

        private async Task<bool> Connect(WalletConnection wc)
        {
            ThirdwebDebug.Log($"Connecting to {wc.provider}...");

            // onConnectionRequested.Invoke(wc);

            await new WaitForSeconds(0.5f);

            try
            {
                _address = await ThirdwebManager.Instance.SDK.wallet.Connect(wc);
                
                // exportButton.SetActive(wc.provider == WalletProvider.LocalWallet);
            }
            catch (Exception e)
            {
                _address = null;
                ThirdwebDebug.LogError($"Failed to connect: {e}");
                onConnectionError.Invoke(e);
                return false;
            }

            PostConnect(wc);

            return true;
        }

        private async void PostConnect(WalletConnection wc = null)
        {
            ThirdwebDebug.Log($"Connected to {_address}");

            // var addy = _address.ShortenAddress();
            // foreach (var addressText in addressTexts)
            //     addressText.text = addy;

            // var bal = await ThirdwebManager.Instance.SDK.wallet.GetBalance();
            // var balStr = $"{bal.value.ToEth()} {bal.symbol}";
            // foreach (var balanceText in balanceTexts)
            //     balanceText.text = balStr;

            // if (wc != null)
            // {
            //     var currentWalletIcon = walletIcons.Find(x => x.provider == wc.provider)?.sprite ?? walletIcons[0].sprite;
            //     foreach (var walletImage in walletImages)
            //         walletImage.sprite = currentWalletIcon;
            // }

            // currentNetworkIcon.sprite = networkIcons.Find(x => x.chain == _currentChainData.identifier)?.sprite ?? networkIcons[0].sprite;
            // currentNetworkText.text = PrettifyNetwork(_currentChainData.identifier);

            onConnected?.Invoke(_address);
        }
    }
}
#endif
