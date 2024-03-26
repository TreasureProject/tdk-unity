using System.Collections;
using Thirdweb.Wallets;
using UnityEngine;
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
                Debug.LogError("Service is null");

            TDKServiceLocator.GetService<TDKThirdwebService>().onConnected.AddListener(address =>
            {
                if(contentHolder.activeInHierarchy)
                    ShowAccountModal();
            });

            Application.targetFrameRate = 60;
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

        public void Show()
        {
            CheckIsConnected();
        }

        private async void CheckIsConnected()
        {
            var isConnected = await TDKServiceLocator.GetService<TDKThirdwebService>().IsConnected();
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

        public void ShowLoginModal()
        {
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

            TDKServiceLocator.GetService<TDKThirdwebService>().Disconnect();
        }

        private void Activate()
        {
            contentHolder.SetActive(true);
            _isActive = true;
        }
    }
}
