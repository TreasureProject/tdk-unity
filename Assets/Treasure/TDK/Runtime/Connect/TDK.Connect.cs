using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using Thirdweb;
using Thirdweb.Unity;

namespace Treasure
{
    public class TDKSilentLoginException : Exception { }

    // Subset of Thirdweb AuthProvider
    public enum SocialAuthProvider
    {
        Google = AuthProvider.Google,
        Apple = AuthProvider.Apple,
        Discord = AuthProvider.Discord
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

            TDKServiceLocator.GetService<TDKThirdwebService>();
        }
    }

    public class Connect
    {
        #region private vars
        private ChainId _chainId = ChainId.Unknown;
        private string _address;
        #endregion

        #region public vars
        public UnityEvent<string> OnConnected = new UnityEvent<string>();
        public UnityEvent<Exception> OnConnectError = new UnityEvent<Exception>();
        public UnityEvent OnDisconnected = new UnityEvent();
        #endregion

        #region accessors / mutators
        public ChainId GetChainId()
        {
            return _chainId;
        }

        public int GetChainIdAsInt()
        {
            return (int)_chainId;
        }

        public string Address
        {
            get { return _address; }
        }
        #endregion

        #region constructors
        public Connect()
        {
            _chainId = TDK.AppConfig.DefaultChainId;
            OnConnected.AddListener(value =>
            {
                HideConnectModal();
            });
        }
        #endregion

        #region private methods
        private async Task ConnectWallet(EcosystemWalletOptions ecosystemWalletOptions, bool isSilentReconnect = false) {
            if (!isSilentReconnect) {
                var authMethod = ecosystemWalletOptions.AuthProvider.ToString();
                if (authMethod == AuthProvider.Default.ToString()) authMethod = "Email/Phone";
                TDKLogger.Log($"[TDK.Connect:ConnectWallet] Connecting via {authMethod}...");
            }
            
            var chainId = GetChainIdAsInt();

            var thirdwebService = TDKServiceLocator.GetService<TDKThirdwebService>();
            await thirdwebService.ConnectWallet(ecosystemWalletOptions, chainId, isSilentReconnect);
            
            _address = await thirdwebService.ActiveWallet.GetAddress();
            OnConnected?.Invoke(_address);
            
            TDK.Analytics.SetTreasureConnectInfo(_address, chainId);
            TDKLogger.LogDebug($"[TDK.Connect:ConnectWallet] Connection success!");
        }

        private async Task Reconnect(EcosystemWalletOptions ecosystemWalletOptions)
        {
            await ConnectWallet(ecosystemWalletOptions, isSilentReconnect: true);
        }
        #endregion

        #region public api
        public async Task<bool> IsWalletConnected()
        {
            return await TDKServiceLocator.GetService<TDKThirdwebService>().IsWalletConnected();
        }

        public async Task SetChainId(ChainId chainId, bool startUserSession = false)
        {
            if (GetChainId() == chainId)
            {
                TDKLogger.Log($"Chain is already set to {chainId}");
                return;
            }

            _chainId = chainId;
            
            var thirdwebService = TDKServiceLocator.GetService<TDKThirdwebService>();
            await thirdwebService.SwitchNetwork(GetChainIdAsInt());

            TDKLogger.Log($"Switched chain to {chainId}");

            if (startUserSession)
            {
                await TDK.Identity.StartUserSession();
            }
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

        public async Task ConnectEmail(string email)
        {
            var ecosystemWalletOptions = new EcosystemWalletOptions(email: email);
            await ConnectWallet(ecosystemWalletOptions);
        }

        public async Task ConnectSocial(SocialAuthProvider provider)
        {
            var ecosystemWalletOptions = new EcosystemWalletOptions(authprovider: (AuthProvider)provider);
            await ConnectWallet(ecosystemWalletOptions);
        }

        public async Task Reconnect(string email) {
            TDKLogger.LogDebug($"[TDK.Connect:Reconnect] Reconnecting email ({email})...");
            var ecosystemWalletOptions = new EcosystemWalletOptions(email: email);
            await Reconnect(ecosystemWalletOptions);
        }

        public async Task Disconnect()
        {
            var thirdwebService = TDKServiceLocator.GetService<TDKThirdwebService>();
            await thirdwebService.DisconnectWallet();
            OnDisconnected?.Invoke();
            _address = null;
            TDK.Analytics.TrackCustomEvent(AnalyticsConstants.EVT_TREASURECONNECT_DISCONNECTED);
        }
        #endregion
    }
}
