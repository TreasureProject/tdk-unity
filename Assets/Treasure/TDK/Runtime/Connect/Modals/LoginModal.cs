using System;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Treasure
{
    public class LoginModal : ModalBase
    {
        [SerializeField] private GameObject socialLoginHolder;
        [SerializeField] private GameObject googleLogin;
        [SerializeField] private GameObject appleLogin;
        [SerializeField] private GameObject discordLogin;
        [SerializeField] private GameObject xLogin;
        [Space]
        [SerializeField] private GameObject loginEmailHolder;
        [SerializeField] private GameObject orSeparatorObject;
        [SerializeField] private GameObject loginWalletHolder;
        [SerializeField] private GameObject landscapeRightSideHolder;
        [Header("Connect with wallet")]
        [SerializeField] private GameObject emailLoginButtons;
        [SerializeField] private GameObject walletLoginButtons;

        [Header("Inputs")]
        [SerializeField] private Button loginGoogleButton;
        [SerializeField] private Button loginAppleButton;
        [SerializeField] private Button loginDiscordButton;
        [SerializeField] private Button loginXButton;
        [SerializeField] private TMP_Text socialsErrorText;
        [Space]
        [SerializeField] private TMP_InputField emailInputField;
        [SerializeField] private TMP_Text errorText;
        [SerializeField] private LayoutElement keyBoardSpace;
        [SerializeField] private Toggle keepMeLoggedInToggle;
        [Space]
        [SerializeField] private Button connectButton;
        [SerializeField] private Button connectWalletButton;
        [SerializeField] private Button connectEmailButton;

        private void Start()
        {
            SetupUI();

            emailInputField.onSelect.AddListener(value => keyBoardSpace.gameObject.SetActive(true));
            emailInputField.onDeselect.AddListener(value => keyBoardSpace.gameObject.SetActive(false));

            connectButton.onClick.AddListener(() => OnClickConnectwithEmail());

            if (connectWalletButton != null)
            {
                connectWalletButton.onClick.AddListener(() =>
                {
                    emailLoginButtons.SetActive(false);
                    walletLoginButtons.SetActive(true);

                    TDK.Analytics.TrackCustomEvent(AnalyticsConstants.EVT_CONNECT_BTN, new System.Collections.Generic.Dictionary<string, object>()
                    {
                        { AnalyticsConstants.PROP_TYPE, "wallet" }
                    });
                });
            }
            if (connectEmailButton != null)
            {
                connectEmailButton.onClick.AddListener(() =>
                {
                    emailLoginButtons.SetActive(true);
                    walletLoginButtons.SetActive(false);

                    TDK.Analytics.TrackCustomEvent(AnalyticsConstants.EVT_CONNECT_BTN, new System.Collections.Generic.Dictionary<string, object>()
                    {
                        { AnalyticsConstants.PROP_TYPE, "email" }
                    });
                });
            }

            loginGoogleButton.onClick.AddListener(() => { ConnectSocial(SocialAuthProvider.Google); });
            loginAppleButton.onClick.AddListener(() => { ConnectSocial(SocialAuthProvider.Apple); });
            loginDiscordButton.onClick.AddListener(() => { ConnectSocial(SocialAuthProvider.Discord); });
            loginXButton.onClick.AddListener(() => { ConnectSocial(SocialAuthProvider.X); });
        }

        private void OnEnable()
        {
            SetupUI();
            connectButton.GetComponent<LoadingButton>().SetLoading(false);
        }

        private async void ConnectSocial(SocialAuthProvider provider)
        {
            if (!TDK.Instance.AbstractedEngineApi.HasInternetConnection())
            {
                errorText.text = "Please make sure you have active Internet connection.";
                errorText.gameObject.SetActive(true);
                return;
            }

            try
            {
                var transitionModal = TDKConnectUIManager.Instance.ShowTransitionModal();
                transitionModal.SetCancelAction(() => TDKConnectUIManager.Instance.ShowLoginModal());
                await TDK.Connect.Disconnect(); // clean up any previous connection attempts
                await TDK.Connect.ConnectSocial(provider);
            }
            catch (Exception ex)
            {
                if (ex.Message != "New connection attempt has been made")
                {
                    TDKLogger.LogException($"[LoginModal:ConnectSocial] Error connecting", ex);
                    // close TransitionModal, go back to login modal and show cause of error
                    TDKConnectUIManager.Instance.ShowLoginModal();
                    socialsErrorText.text = ex.Message;
                    socialsErrorText.gameObject.SetActive(true);
                }
            }
        }

        private void SetupUI()
        {
            socialLoginHolder.SetActive(true);
            googleLogin.SetActive(true);
            appleLogin.SetActive(true);
            discordLogin.SetActive(true);
            xLogin.SetActive(true);
            loginEmailHolder.SetActive(true);
            loginWalletHolder.SetActive(false);

            if (landscapeRightSideHolder != null)
            {
                landscapeRightSideHolder.SetActive(true);
                orSeparatorObject.SetActive(true);
            }
            else
            {
                orSeparatorObject.SetActive(false);
            }
        }

        private async void OnClickConnectwithEmail()
        {
            if (!TDK.Instance.AbstractedEngineApi.HasInternetConnection())
            {
                errorText.text = "Please make sure you have active Internet connection.";
                errorText.gameObject.SetActive(true);
                return;
            }

            if (ValidateEmail(emailInputField.text, out var message))
            {
                errorText.gameObject.SetActive(false);
                // StartCoroutine(WaitToGoToConfirmLogin());

                connectButton.GetComponent<LoadingButton>().SetLoading(true);

                try
                {
                    await TDK.Connect.Disconnect(); // clean up any previous connection attempts
                    await TDK.Connect.ConnectEmail(emailInputField.text);
                }
                catch (Exception e)
                {
                    TDKLogger.LogException($"[LoginModal:OnClickConnectwithEmail] {e.Message}", e);

                    // Ignore error display if user purposely closed the verification modal
                    if (e.Message != "User closed modal" && e.Message != "User cancelled")
                    {
                        errorText.text = e.Message;
                        errorText.gameObject.SetActive(true);
                    }
                }

                connectButton.GetComponent<LoadingButton>().SetLoading(false);
            }
            else
            {
                errorText.text = message;
                errorText.gameObject.SetActive(true);
            }
        }

        // test code
        // IEnumerator WaitToGoToConfirmLogin()
        // {
        //     connectButton.GetComponent<LoadingButton>().SetLoading(true);
        //     yield return new WaitForSeconds(2.5f);
        //     connectButton.GetComponent<LoadingButton>().SetLoading(false);
        //     UIManager.Instance.ShowConfirmLoginModal();
        // }

        private bool ValidateEmail(string email, out string errorMessage)
        {
            if (string.IsNullOrEmpty(email))
            {
                errorMessage = "Please enter email address";
                return false;
            }

            email = email.ToLower();
            var matchEmailPattern = "^[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?$";

            var match = Regex.Match(email, matchEmailPattern, RegexOptions.None, TimeSpan.FromSeconds(2));
            var isEmailValid = match.Length == email.Length;
            if (!isEmailValid)
            {
                errorMessage = "Please enter a valid email";
                return false;
            }

            errorMessage = null;
            return true;
        }
    }
}
