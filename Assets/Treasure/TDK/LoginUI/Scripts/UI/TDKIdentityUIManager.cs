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

        [Header("Modals")]
        [SerializeField] private ModalBase loginModal;
        [SerializeField] private ModalBase confirmLoginModal;
        [SerializeField] private Button backGroundButton;
        [Header("Test buttons")]
        [SerializeField] private Button switchThemeButton;
        [SerializeField] private Button switchSceneButton;
        [SerializeField] private ScreenOrientation currentOriantation;
        [Header("Active object")]
        [SerializeField] private GameObject logedInHolder;

        private ModalBase currentModalOpended;

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
                HideUI();
            });

            currentModalOpended = loginModal;
            if (TDKServiceLocator.GetService<TDKThirdwebService>() == null)
                Debug.LogError("Service is null");

            TDKServiceLocator.GetService<TDKThirdwebService>().onConnected.AddListener(address =>
            {
                ShowAccountModal();
            });

            Application.targetFrameRate = 60;
        }

        private void OnEnable()
        {
            if (currentModalOpended != null)
                currentModalOpended.Hide();

            loginModal.Show();
            currentModalOpended = loginModal;
        }

        //test code
        IEnumerator SwitchScene()
        {
            switchSceneButton.interactable = false;
            Screen.orientation = currentOriantation == ScreenOrientation.Portrait ? ScreenOrientation.LandscapeLeft : ScreenOrientation.Portrait;
            yield return null;
            SceneManager.LoadScene(currentOriantation == ScreenOrientation.Portrait ? 1 : 0);
        }

        public void ShowLoginModal()
        {
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
            if (currentModalOpended != null)
                currentModalOpended.Hide();

            logedInHolder.SetActive(true);
        }

        public void LogOut()
        {
            logedInHolder.SetActive(false);

            loginModal.Show();
            currentModalOpended = loginModal;
        }

        public void HideUI()
        {
            if (currentModalOpended != null)
                currentModalOpended.Hide();
            currentModalOpended = null;
            gameObject.SetActive(false);
        }
    }
}
