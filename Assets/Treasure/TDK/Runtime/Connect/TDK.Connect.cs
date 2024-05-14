using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

#if TDK_THIRDWEB
using Thirdweb;
#endif

namespace Treasure
{
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
        #region private vars
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
        public async Task<ChainId> GetChainId()
        {
#if TDK_THIRDWEB
            if (_chainId == ChainId.Unknown)
            {
                _chainId = (ChainId)(int)await TDKServiceLocator.GetService<TDKThirdwebService>().Wallet.GetChainId();
            }

            return _chainId;
#else
            return await Task.FromResult(ChainId.Arbitrum);
#endif
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
        public Connect() { }
        #endregion

        #region private methods
        private async Task ConnectWallet(WalletConnection wc, ChainId chainId)
        {
            TDKLogger.Log($"[TDK.Connect:Connect] Connecting to {wc.provider}...");
            var result = await TDKServiceLocator.GetService<TDKThirdwebService>().Wallet.Connect(wc);
            _address = result;
            _email = wc.email;
            OnConnected?.Invoke(_address);
            TDK.Analytics.SetTreasureConnectInfo(_address, (int)chainId);
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
            await TDKServiceLocator.GetService<TDKThirdwebService>().Wallet.SwitchNetwork((int)chainId);
            _chainId = chainId;
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

        public async Task ConnectEmail(string email)
        {
            var chainId = await GetChainId();
            var wc = new WalletConnection(
                    provider: WalletProvider.SmartWallet,
                    chainId: (int)chainId,
                    email: email,
                    authOptions: new AuthOptions(AuthProvider.EmailOTP),
                    personalWallet: WalletProvider.InAppWallet
                );
            await ConnectWallet(wc, chainId);
        }

        public async Task Disconnect(bool endSession = false)
        {
            if (await IsWalletConnected())
            {
                await TDKServiceLocator.GetService<TDKThirdwebService>().Wallet.Disconnect(endSession);
                OnDisconnected?.Invoke();
            }

            _address = null;
            _email = null;
            TDK.Analytics.TrackCustomEvent(AnalyticsConstants.EVT_TREASURECONNECT_DISCONNECTED);
        }
        #endregion
    }
}
