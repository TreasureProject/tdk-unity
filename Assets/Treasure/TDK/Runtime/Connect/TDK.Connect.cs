using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

#if TDK_THIRDWEB
using Thirdweb;
#endif

namespace Treasure
{
    public class TDKSilentLoginException : Exception { }

    // Subset of Thirdweb AuthProvider
    public enum SocialAuthProvider
    {
        Google = 1,
        Apple = 2,
        Facebook = 3
    }

    public partial class TDK : MonoBehaviour
    {
        public static Connect Connect { get; private set; }

        /// <summary>
        /// Initialize the Connect module
        /// </summary>
        private void InitConnect()
        {
            Connect = new Connect();

#if TDK_THIRDWEB
            TDKServiceLocator.GetService<TDKThirdwebService>();
#endif
        }
    }

    public class Connect
    {
        public struct Options
        {
            public bool isSilent;
        }

        #region private vars
        private Options? _options;
        private ChainId _chainId = ChainId.Unknown;
        private string _email;
        private string _address;
        #endregion

        #region public vars
        public UnityEvent<string> OnConnected = new UnityEvent<string>();
        public UnityEvent<Exception> OnConnectError = new UnityEvent<Exception>();
        public UnityEvent OnDisconnected = new UnityEvent();
        #endregion

        #region accessors / mutators
        public bool IsSilent
        {
            get { return _options.HasValue && _options.Value.isSilent; }
        }

        public async Task<ChainId> GetChainId()
        {
            if (_chainId == ChainId.Unknown)
            {
#if TDK_THIRDWEB
                // WebGL version of the Thirdweb SDK requires a wallet to be connected to call GetChainId()
                var isConnected = await IsWalletConnected();
                if (Utils.IsWebGLBuild() && !isConnected)
                {
                    var defaultChainIdentifier = TDK.Instance.AppConfig.GetModuleConfig<TDKThirdwebConfig>().DefaultChainIdentifier;
                    _chainId = Constants.NameToChainId[defaultChainIdentifier];
                }
                else
                {
                    _chainId = (ChainId)(int)await TDKServiceLocator.GetService<TDKThirdwebService>().Wallet.GetChainId();
                }
#else
                _chainId = ChainId.Arbitrum;
#endif
            }

            return _chainId;
        }

        public string Email
        {
            get { return _email; }
        }

        public string Address
        {
            get { return _address; }
        }
        #endregion

        #region constructors
        public Connect()
        {
            OnConnected.AddListener(value =>
            {
                HideConnectModal();
            });
        }
        #endregion

        #region private methods
        private async Task ConnectWallet(WalletConnection wc, ChainId chainId)
        {
#if TDK_THIRDWEB
            TDKLogger.Log($"[TDK.Connect:Connect] Connecting to {wc.provider}...");
            var result = await TDKServiceLocator.GetService<TDKThirdwebService>().Wallet.Connect(wc);
            _address = result;
            _email = wc.email;
            OnConnected?.Invoke(_address);
            TDK.Analytics.SetTreasureConnectInfo(_address, (int)chainId);
#else
            TDKLogger.LogError("Unable to connect wallet. TDK Connect wallet service not implemented.");
#endif
        }
        #endregion

        #region public api
        public async Task<bool> IsWalletConnected()
        {
#if TDK_THIRDWEB
            return await TDKServiceLocator.GetService<TDKThirdwebService>().Wallet.IsConnected();
#else
            return await Task.FromResult(false);
#endif
        }

        public async Task SetChainId(ChainId chainId)
        {
            if (_chainId == chainId)
            {
                TDKLogger.Log($"Chain is already set to {chainId}");
                return;
            }

            _chainId = chainId;

#if TDK_THIRDWEB
            // Thirdweb SDK currently doesn't allow you to switch networks while connected to a smart wallet
            // Reinitialize it and auto-connect instead
            var connectedEmail = _email;
            TDKServiceLocator.GetService<TDKThirdwebService>().InitializeSDK(Constants.ChainIdToName[chainId]);
            if (!string.IsNullOrEmpty(connectedEmail))
            {
                await ConnectEmail(connectedEmail, new Options { isSilent = true });
            }
#endif

            TDKLogger.Log($"Switched chain to {chainId}");
        }

        public void ShowConnectModal()
        {
            if (_address != null)
            {
                TDKConnectUIManager.Instance.ShowAccountModal();
            }
            else
            {
                TDKConnectUIManager.Instance.ShowLoginModal();
            }
        }

        public void HideConnectModal()
        {
            TDKConnectUIManager.Instance.Hide();
        }

        public async Task ConnectEmail(string email, Options? options = null)
        {
#if TDK_THIRDWEB
            _options = options;
            var chainId = await GetChainId();
            var wc = new WalletConnection(
                    provider: WalletProvider.SmartWallet,
                    chainId: (int)chainId,
                    email: email,
                    authOptions: new AuthOptions(AuthProvider.EmailOTP),
                    personalWallet: WalletProvider.InAppWallet
                );
            await ConnectWallet(wc, chainId);
#else
            TDKLogger.LogError("Unable to connect email. TDK Connect wallet service not implemented.");
#endif
        }

        public async Task ConnectSocial(SocialAuthProvider provider)
        {
#if TDK_THIRDWEB
            var chainId = await GetChainId();
            var wc = new WalletConnection(
                provider: WalletProvider.SmartWallet,
                chainId: (int)chainId,
                authOptions: new AuthOptions((AuthProvider)provider),
                personalWallet: WalletProvider.InAppWallet
            );
            await ConnectWallet(wc, chainId);
#else
            TDKLogger.LogError("Unable to connect social. TDK Connect wallet service not implemented.");
#endif
        }

        public async Task Disconnect(bool endSession = false)
        {
#if TDK_THIRDWEB
            if (await IsWalletConnected())
            {
                await TDKServiceLocator.GetService<TDKThirdwebService>().Wallet.Disconnect(endSession);
                OnDisconnected?.Invoke();
            }
#endif

            _address = null;
            _email = null;
            TDK.Analytics.TrackCustomEvent(AnalyticsConstants.EVT_TREASURECONNECT_DISCONNECTED);
        }
        #endregion
    }
}
