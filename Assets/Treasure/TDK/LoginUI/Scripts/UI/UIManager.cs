using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Treasure
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance = null;

        [SerializeField] private ModalBase loginModal;
        [SerializeField] private ModalBase confirmLoginModal;
        [Space]
        [SerializeField] private Button switchThemeButton;
        [SerializeField] private Button switchSceneButton;
        [SerializeField] private ScreenOrientation currentOriantation;
        [Space]
        [SerializeField] private GameObject logedInHolder;

        private ModalBase currentModalOpended;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;

            Application.targetFrameRate = 60;
        }

        private void Start()
        {
            switchThemeButton.onClick.AddListener(() => { ThemeManager.Instance.ChangeTheme(); });
            switchSceneButton.onClick.AddListener(() =>
            {
                StartCoroutine(SwitchScene());
            });

            currentModalOpended = loginModal;
        }

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
