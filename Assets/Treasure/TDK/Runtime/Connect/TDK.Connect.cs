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
        Discord = AuthProvider.Discord,
        X = AuthProvider.X
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
        public UnityEvent<string> OnAddressChanged = new UnityEvent<string>(); // for cross-chain switching
        public UnityEvent OnDisconnected = new UnityEvent();
        #endregion

        #region accessors / mutators
        public ChainId ChainId
        {
            get { return _chainId; }
        }

        public int ChainIdNumber
        {
            get { return (int)_chainId; }
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
        private async Task ConnectWallet(EcosystemWalletOptions ecosystemWalletOptions, bool isSilentReconnect = false)
        {
            if (TDK.Identity.IsUsingTreasureLauncher)
            {
                TDKLogger.Log("[TDK.Connect:ConnectWallet] Using launcher token, skipping");
                return;
            }
            if (!isSilentReconnect)
            {
                var authMethod = ecosystemWalletOptions.AuthProvider.ToString();
                if (authMethod == AuthProvider.Default.ToString()) authMethod = "Email/Phone";
                TDKLogger.Log($"[TDK.Connect:ConnectWallet] Connecting via {authMethod}...");
            }

            var thirdwebService = TDKServiceLocator.GetService<TDKThirdwebService>();
            await thirdwebService.ConnectWallet(ecosystemWalletOptions, ChainIdNumber, isSilentReconnect);

            await UpdateConnectInfo(ChainId);
            TDKLogger.LogDebug($"[TDK.Connect:ConnectWallet] Connection success!");
        }

        private async Task Reconnect(EcosystemWalletOptions ecosystemWalletOptions)
        {
            await ConnectWallet(ecosystemWalletOptions, isSilentReconnect: true);
        }

        private async Task UpdateConnectInfo(ChainId chainId, bool newConnection = true)
        {
            var thirdwebService = TDKServiceLocator.GetService<TDKThirdwebService>();
            var oldAddress = _address;
            _address = await thirdwebService.ActiveWallet.GetAddress();
            if (newConnection)
            {
                OnConnected?.Invoke(_address);
                thirdwebService.TrackThirdwebAnalytics("connectWallet", "connect", "smartWallet", _address);
            }
            if (oldAddress != _address)
            {
                OnAddressChanged?.Invoke(_address);
            }
            TDK.Analytics.SetTreasureConnectInfo(_address, (int) chainId, newConnection);
        }
        #endregion

        #region public api
        public async Task<bool> IsWalletConnected()
        {
            return await TDKServiceLocator.GetService<TDKThirdwebService>().IsWalletConnected();
        }

        public async Task SetChainId(ChainId chainId, bool startUserSession = false)
        {
            if (TDK.Identity.IsUsingTreasureLauncher)
            {
                TDKLogger.Log("[TDK.Connect:SetChainId] Using launcher token, skipping");
                return;
            }

            if (chainId == ChainId)
            {
                TDKLogger.Log($"Chain is already set to {chainId}");
                return;
            }

            var thirdwebService = TDKServiceLocator.GetService<TDKThirdwebService>();
            await thirdwebService.SwitchNetwork((int) chainId);

            if (await thirdwebService.IsWalletConnected())
            {
                await UpdateConnectInfo(chainId, newConnection: false);
            }

            _chainId = chainId;

            TDKLogger.Log($"Switched chain to {chainId}");

            if (startUserSession)
            {
                await TDK.Identity.StartUserSession();
            }
        }

        public void ShowConnectModal()
        {
            if (TDK.Identity.Address != null)
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
            if (TDKConnectUIManager.Instance != null)
            {
                TDKConnectUIManager.Instance.Hide();
            }
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

        public async Task ConnectExternalWallet()
        {
            if (TDK.Identity.IsUsingTreasureLauncher)
            {
                TDKLogger.Log("[TDK.Connect:ConnectExternalWallet] Using launcher token, skipping");
                return;
            }

            var thirdwebService = TDKServiceLocator.GetService<TDKThirdwebService>();
            await thirdwebService.ConnectExternalWallet(ChainIdNumber);

            await UpdateConnectInfo(ChainId);
            TDKLogger.LogDebug($"[TDK.Connect:ConnectExternalWallet] Connection success!");
        }

        public async Task<bool> ConnectViaCookie(string authCookie, AuthProvider authProvider, string email = null)
        {
            var thirdwebService = TDKServiceLocator.GetService<TDKThirdwebService>();
            var ecosystemWalletOptions = new EcosystemWalletOptions(authprovider: authProvider, email: email);

            await thirdwebService.ConnectWallet(
                ecosystemWalletOptions,
                ChainIdNumber,
                isSilentReconnect: true,
                authCookie
            );
            if (await thirdwebService.IsWalletConnected())
            {
                await UpdateConnectInfo(ChainId);
                TDKLogger.LogDebug($"[TDK.Connect:ConnectViaLauncherCookie] Connection success!");
                return true;
            }
            return false;
        }

        public async Task Reconnect(string email)
        {
            TDKLogger.LogDebug($"[TDK.Connect:Reconnect] Reconnecting email ({email})...");
            var ecosystemWalletOptions = new EcosystemWalletOptions(email: email);
            await Reconnect(ecosystemWalletOptions);
        }

        public async Task Disconnect()
        {
            if (TDK.Identity.IsUsingTreasureLauncher)
            {
                TDKLogger.Log("[TDK.Connect:Disconnect] Using launcher token, skipping.");
                return;
            }
            var thirdwebService = TDKServiceLocator.GetService<TDKThirdwebService>();
            await thirdwebService.DisconnectWallet();
            OnDisconnected?.Invoke();
            _address = null;
            TDK.Analytics.TrackCustomEvent(AnalyticsConstants.EVT_TREASURECONNECT_DISCONNECTED);
        }
        #endregion
    }
}
