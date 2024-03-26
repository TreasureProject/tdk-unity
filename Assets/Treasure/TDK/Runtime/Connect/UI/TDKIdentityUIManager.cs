using System;
using System.Collections;
using System.Numerics;
using System.Threading.Tasks;
using Thirdweb;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Treasure
{
    public class TDKIdentityUIManager : MonoBehaviour
    {
        public static TDKIdentityUIManager Instance = null;

        [SerializeField] private GameObject contentHolder;
        [Header("Modals")]
        [SerializeField] private ModalBase loginModal;
        [SerializeField] private ModalBase confirmLoginModal;
        [SerializeField] private ModalBase logedInHolder;
        [SerializeField] private Button backGroundButton;
        [Header("Test buttons")]
        [SerializeField] private Button switchThemeButton;
        [SerializeField] private Button switchSceneButton;
        [SerializeField] private ScreenOrientation currentOriantation;       

        private ModalBase currentModalOpended;

        private bool _isActive = false;

        public UnityEvent<Exception> onConnectionError = new UnityEvent<Exception>();
        public UnityEvent<string> onConnected = new UnityEvent<string>();

        private string _address;
        private string _email;
        private bool useSmartWallets = true;
        private ChainData _currentChainData;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;          
        }

        private void Start()
        {
            switchThemeButton.onClick.AddListener(() => { ThemeManager.Instance.ChangeTheme(); });
            switchSceneButton.onClick.AddListener(() =>
            {
                StartCoroutine(SwitchScene());
            });
            backGroundButton.onClick.AddListener(() =>
            {
                Hide();
            });

            if (TDKServiceLocator.GetService<TDKThirdwebService>() == null)
                TDKLogger.LogError("[TDKIdentityUIManager:Start] Service is null");

            _currentChainData = ThirdwebManager.Instance.supportedChains.Find(x => x.identifier == ThirdwebManager.Instance.activeChain);

            onConnected.AddListener(value =>
            {
                ShowAccountModal();
            });
        }

        #region test code
        IEnumerator SwitchScene()
        {
            switchSceneButton.interactable = false;
            Screen.orientation = currentOriantation == ScreenOrientation.Portrait ? ScreenOrientation.LandscapeLeft : ScreenOrientation.Portrait;
            yield return null;
            SceneManager.LoadScene(currentOriantation == ScreenOrientation.Portrait ? 1 : 0);
        }
        #endregion

        #region Show and Hide
        public void Show()
        {
            CheckIsConnected();
        }

        private async void CheckIsConnected()
        {
            var isConnected = await IsConnected();
            if (isConnected)
                ShowAccountModal();
            else
                ShowLoginModal();
        }

        public void Hide()
        {
            if (currentModalOpended != null)
                currentModalOpended.Hide();

            currentModalOpended = null;

            contentHolder.SetActive(false);
            _isActive = false;
        }
        #endregion

        #region Changing modals
        public void ShowLoginModal(bool disconnect = false)
        {
            if (disconnect)
                Disconnect();

            Activate();
            if (currentModalOpended != null)
                currentModalOpended.Hide();

            loginModal.Show();
            currentModalOpended = loginModal;        
        }

        public void ShowConfirmLoginModal()
        {
            if (currentModalOpended != null)
                currentModalOpended.Hide();

            confirmLoginModal.Show();
            currentModalOpended = confirmLoginModal;
        }

        public void ShowAccountModal()
        {
            Activate();
            if (currentModalOpended != null)
                currentModalOpended.Hide();

            currentModalOpended = logedInHolder;
            logedInHolder.Show();         
        }

        public void LogOut()
        {
            logedInHolder.Hide();

            loginModal.Show();
            currentModalOpended = loginModal;

            Disconnect();
        }

        private void Activate()
        {
            contentHolder.SetActive(true);
            _isActive = true;
        }
        #endregion

        #region Connecting
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

        private async Task<bool> Connect(WalletConnection wc)
        {
            TDKLogger.Log($"[TDKIdentityUIManager:Connect] Connecting to {wc.provider}...");

            await new WaitForSeconds(0.5f);

            try
            {
                _address = await ThirdwebManager.Instance.SDK.Wallet.Connect(wc);
            }
            catch (Exception e)
            {
                _address = null;
                TDKLogger.LogError($"[TDKIdentityUIManager:Connect] error: {e}");
                onConnectionError?.Invoke(e);
                return false;
            }

            PostConnect(wc);

            return true;
        }

        private async void PostConnect(WalletConnection wc = null)
        {
            TDKLogger.Log($"[TDKIdentityUIManager:PostConnect] address: {_address}");
            onConnected?.Invoke(_address);

            var chainId = await TDK.Identity.GetChainId();
            TDK.Analytics.SetTreasureConnectInfo(_address, (int)chainId);
        }

        public async Task<bool> IsConnected()
        {
            return await ThirdwebManager.Instance.SDK.Wallet.IsConnected();
        }

        // public string GetWalletAddress()
        // {
        //     return _address;
        // }

        public string GetUserEmail()
        {
            return _email;
        }

        public async void Disconnect(bool endSession = false)
        {
            await ThirdwebManager.Instance.SDK.Wallet.Disconnect(endSession);
        }
        #endregion
    }
}
