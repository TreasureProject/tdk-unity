using System;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Treasure
{
    public class LoginModal : ModalBase
    {
        [SerializeField] private AppSettingsData appSettingsData;
        [Space]
        [SerializeField] private GameObject socialLoginHolder;
        [SerializeField] private GameObject googleLogin;
        [SerializeField] private GameObject appleLogin;
        [SerializeField] private GameObject facebookLogin;
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
        [SerializeField] private Button loginFacebookButton;
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
            SetupFromSettings();

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
            loginFacebookButton.onClick.AddListener(() => { ConnectSocial(SocialAuthProvider.Facebook); });
        }

        private void OnEnable()
        {
            SetupFromSettings();
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
                await TDK.Connect.Disconnect(); // clean up any previous connection attempts
                await TDK.Connect.ConnectSocial(provider);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
            }
        }

        private void SetupFromSettings()
        {
            socialLoginHolder.SetActive(appSettingsData.loginSettings.HasSocialLogin());
            googleLogin.SetActive(appSettingsData.loginSettings.hasGoogleLogin);
            appleLogin.SetActive(appSettingsData.loginSettings.hasAppleLogin);
            facebookLogin.SetActive(appSettingsData.loginSettings.hasFacebookLogin);

            loginEmailHolder.SetActive(appSettingsData.loginSettings.hasEmailLogin);
            loginWalletHolder.SetActive(appSettingsData.loginSettings.hasWalletLogin);

            if (landscapeRightSideHolder != null)
            {
                landscapeRightSideHolder.SetActive(appSettingsData.loginSettings.HasSocialLogin() || appSettingsData.loginSettings.hasEmailLogin);
                orSeparatorObject.SetActive(appSettingsData.loginSettings.HasSocialLogin() && appSettingsData.loginSettings.hasEmailLogin);
            }
            else
            {
                orSeparatorObject.SetActive(appSettingsData.loginSettings.hasWalletLogin && appSettingsData.loginSettings.hasEmailLogin);
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
                    TDKLogger.LogError($"[LoginModal:OnClickConnectwithEmail] {e.Message}");

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
