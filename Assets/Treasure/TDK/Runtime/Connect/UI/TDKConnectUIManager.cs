using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Treasure
{
    public class TDKConnectUIManager : MonoBehaviour
    {
        public static TDKConnectUIManager Instance = null;

        [SerializeField] private GameObject contentHolder;
        [Header("Modals")]
        [SerializeField] private ModalBase loginModal;
        [SerializeField] private ConfirmLoginModal confirmLoginModal;
        [SerializeField] private AccountModal accountModal;
        [SerializeField] private TransitionModal transitionModal;
        [SerializeField] private Button backGroundButton;
        [Header("Test buttons")]
        [SerializeField] private Button switchThemeButton;
        [SerializeField] private Button switchSceneButton;
        [SerializeField] private ScreenOrientation currentOriantation;

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
                if (TDK.AppConfig.ConnectHideBehavior == TDKConfig.ConnectUIHideBehavior.HideOnOutsideClick)
                {
                    Hide();
                }
                else if (TDK.AppConfig.ConnectHideBehavior == TDKConfig.ConnectUIHideBehavior.DoNotHideOnOtpScreen)
                {
                    // do not hide while waiting for user to input OTP
                    if (currentModalOpended != confirmLoginModal)
                    {
                        Hide();
                    }
                }
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

        #region Changing modals
        public void ShowLoginModal()
        {
            Activate();
            if (currentModalOpended != null)
                currentModalOpended.Hide();

            loginModal.Show();
            currentModalOpended = loginModal;

            TDK.Analytics.TrackCustomEvent(AnalyticsConstants.EVT_TREASURECONNECT_UI_LOGIN);
        }

        public ConfirmLoginModal ShowOtpModal(string email)
        {
            if (currentModalOpended != null)
                currentModalOpended.Hide();

            confirmLoginModal.SetEmail(email);
            confirmLoginModal.Show();
            currentModalOpended = confirmLoginModal;

            TDK.Analytics.TrackCustomEvent(AnalyticsConstants.EVT_TREASURECONNECT_UI_CONFIRM);

            return confirmLoginModal;
        }

        public void ShowAccountModal()
        {
            Activate();
            if (currentModalOpended != null)
                currentModalOpended.Hide();

            currentModalOpended = accountModal;
            if (TDK.Identity.IsUsingTreasureLauncher)
            {
                accountModal.SetDisconnectButtonVisible(false);
            }
            accountModal.Show();

            TDK.Analytics.TrackCustomEvent(AnalyticsConstants.EVT_TREASURECONNECT_UI_ACCOUNT);
        }

        public TransitionModal ShowTransitionModal()
        {
            if (currentModalOpended != null)
                currentModalOpended.Hide();

            transitionModal.Show();
            currentModalOpended = transitionModal;

            return transitionModal;
        }

        public void Hide()
        {
            if (currentModalOpended != null)
                currentModalOpended.Hide();

            if (currentModalOpended == confirmLoginModal)
                confirmLoginModal.CancelCurrentLoginAttempt();

            currentModalOpended = null;

            contentHolder.SetActive(false);
        }

        public void LogOut()
        {
            accountModal.Hide();

            loginModal.Show();
            currentModalOpended = loginModal;
        }

        private void Activate()
        {
            contentHolder.SetActive(true);
        }
        #endregion
    }
}
