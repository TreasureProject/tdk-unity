using System.Collections;
using Thirdweb.Wallets;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Treasure
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance = null;

        [Header("Modals")]
        [SerializeField] private ModalBase loginModal;
        [SerializeField] private ModalBase confirmLoginModal;
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

            currentModalOpended = loginModal;
            if (TDKServiceLocator.GetService<TDKThirdwebService>() == null)
                Debug.LogError("Service is null");

            TDKServiceLocator.GetService<TDKThirdwebService>().onConnected.AddListener(address =>
            {
                //(TODO) check address variable
                ShowLoggedInView();
            });

            Application.targetFrameRate = 60;
        }

        //test code
        IEnumerator SwitchScene()
        {
            switchSceneButton.interactable = false;
            Screen.orientation = currentOriantation == ScreenOrientation.Portrait ? ScreenOrientation.LandscapeLeft : ScreenOrientation.Portrait;
            yield return null;
            SceneManager.LoadScene(currentOriantation == ScreenOrientation.Portrait ? 1 : 0);
        }

        public void ShowConfirmLoginModal()
        {
            currentModalOpended.Hide();
            confirmLoginModal.Show();
            currentModalOpended = confirmLoginModal;
        }

        public void ShowLoggedInView()
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
    }
}
