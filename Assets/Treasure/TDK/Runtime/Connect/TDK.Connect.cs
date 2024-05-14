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
                _chainId = (ChainId)(int)await ThirdwebManager.Instance.SDK.Wallet.GetChainId();
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

        #region public api
        public async Task<bool> IsConnected()
        {
#if TDK_THIRDWEB
            return await ThirdwebManager.Instance.SDK.Wallet.IsConnected();
#else
            return await Task.FromResult(false);
#endif
        }

        public async Task SetChainId(ChainId chainId)
        {
            await ThirdwebManager.Instance.SDK.Wallet.SwitchNetwork((int)chainId);
            _chainId = chainId;
        }

        public async void ShowConnectModal()
        {
            if (await IsConnected())
            {
                TDKConnectUIManager.Instance.ShowAccountModal();
            }
            else
            {
                TDKConnectUIManager.Instance.ShowLoginModal();
            }
        }

        public async Task<bool> ConnectEmail(string email)
        {
            _email = email;

            var chainId = await GetChainId();
            var wc = new WalletConnection(
                    provider: WalletProvider.SmartWallet,
                    chainId: (int)chainId,
                    email: email,
                    authOptions: new AuthOptions(AuthProvider.EmailOTP),
                    personalWallet: WalletProvider.InAppWallet
                );
            return await ConnectWallet(wc, chainId);
        }

        private async Task<bool> ConnectWallet(WalletConnection wc, ChainId chainId)
        {
            TDKLogger.Log($"[TDK.Connect:Connect] Connecting to {wc.provider}...");

            try
            {
                _address = await ThirdwebManager.Instance.SDK.Wallet.Connect(wc);
            }
            catch (Exception e)
            {
                _address = null;
                _email = null;
                TDKLogger.LogError($"[TDK.Connect:Connect] Error occurred: {e}");
                OnConnectError?.Invoke(e);
                return false;
            }

            OnConnected?.Invoke(_address);
            TDK.Analytics.SetTreasureConnectInfo(_address, (int)chainId);
            return true;
        }

        public async Task<bool> Disconnect(bool endSession = false)
        {
            try
            {
                await ThirdwebManager.Instance.SDK.Wallet.Disconnect(endSession);
            }
            catch (Exception e)
            {
                TDKLogger.LogError($"[TDK.Connect:Disconnect] Error occurred: {e}");
                return false;
            }

            _address = null;
            _email = null;
            OnDisconnected?.Invoke();
            TDK.Analytics.TrackCustomEvent(AnalyticsConstants.EVT_TREASURECONNECT_DISCONNECTED);
            return true;
        }
        #endregion
    }
}
