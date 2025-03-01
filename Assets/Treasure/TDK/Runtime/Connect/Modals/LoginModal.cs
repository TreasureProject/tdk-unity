using System;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Treasure
{
    public class LoginModal : ModalBase
    {
        [Header("Inputs")]
        [SerializeField] private Button loginGoogleButton;
        [SerializeField] private Button loginAppleButton;
        [SerializeField] private Button loginDiscordButton;
        [SerializeField] private Button loginXButton;
        [SerializeField] private Button loginWalletButton;
        [SerializeField] private TMP_Text socialsErrorText;
        [Space]
        [SerializeField] private TMP_InputField emailInputField;
        [SerializeField] private TMP_Text errorText;
        [SerializeField] private LayoutElement keyBoardSpace;
        [SerializeField] private Toggle keepMeLoggedInToggle;
        [Space]
        [SerializeField] private Button connectButton;

        private void Start()
        {
            emailInputField.onSelect.AddListener(value => keyBoardSpace.gameObject.SetActive(true));
            emailInputField.onDeselect.AddListener(value => keyBoardSpace.gameObject.SetActive(false));

            connectButton.onClick.AddListener(() => OnClickConnectwithEmail());

            loginGoogleButton.onClick.AddListener(() => { ConnectSocial(SocialAuthProvider.Google); });
            loginAppleButton.onClick.AddListener(() => { ConnectSocial(SocialAuthProvider.Apple); });
            loginDiscordButton.onClick.AddListener(() => { ConnectSocial(SocialAuthProvider.Discord); });
            loginXButton.onClick.AddListener(() => { ConnectSocial(SocialAuthProvider.X); });

            loginWalletButton.gameObject.SetActive(TDK.AppConfig.EnableWalletLogin);
            loginWalletButton.onClick.AddListener(() => { ConnectExternalWallet(); });
        }

        private void OnEnable()
        {
            connectButton.GetComponent<LoadingButton>().SetLoading(false);
        }

        private async void ConnectSocial(SocialAuthProvider provider)
        {
            if (!TDK.Instance.AbstractedEngineApi.HasInternetConnection())
            {
                socialsErrorText.text = "Please make sure you have active Internet connection.";
                socialsErrorText.gameObject.SetActive(true);
                return;
            }

            TDKConnectUIManager.Instance.ShowTransitionModal(
                "Authenticating...",
                "Sign into your account in the pop-up",
                buttonText: "Cancel",
                buttonAction: () => TDKConnectUIManager.Instance.ShowLoginModal()
            );
            try
            {
                await TDK.Connect.Disconnect(); // clean up any previous connection attempts
                await TDK.Connect.ConnectSocial(provider);
            }
            catch (Exception ex)
            {
                if (ex.Message != "New connection attempt has been made")
                {
                    TDKLogger.LogException($"[LoginModal:ConnectSocial] Error connecting", ex);
                    if (TDKConnectUIManager.Instance.GetTransitionModal().gameObject.activeInHierarchy) // if transition modal is still open
                    {
                        // close TransitionModal, go back to login modal and show cause of error
                        TDKConnectUIManager.Instance.ShowLoginModal();
                        socialsErrorText.text = ex.Message;
                        socialsErrorText.gameObject.SetActive(true);
                    }
                }
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

        [DllImport("__Internal")]
        private static extern void OpenConnectModal();

        private async void ConnectExternalWallet()
        {
            if (!TDK.Instance.AbstractedEngineApi.HasInternetConnection())
            {
                socialsErrorText.text = "Please make sure you have active Internet connection.";
                socialsErrorText.gameObject.SetActive(true);
                return;
            }

            var thirdwebService = TDKServiceLocator.GetService<TDKThirdwebService>();
            thirdwebService.EnsureWalletConnectInitialized();

            var ensureChainModalTaskSource = new TaskCompletionSource<object>();
            TDKConnectUIManager.Instance.ShowTransitionModal(
                "Confirm selected network",
                $"For better results, make sure the active network ({TDK.Connect.ChainId}) is selected in your external wallet app before connecting",
                buttonText: "Continue",
                buttonAction: () => {
                    ensureChainModalTaskSource.SetResult(null);
                }
            );
            await ensureChainModalTaskSource.Task;
            var transitionModal = TDKConnectUIManager.Instance.GetTransitionModal();
            transitionModal.SetInfoLabels("Processing...", "This could take a few seconds");
            transitionModal.SetButtonAction("Restart", () => TDKConnectUIManager.Instance.ShowLoginModal());
            var isWalletConnectReady = await thirdwebService.WaitForWalletConnectReady(maxWait: 10);
            if (!isWalletConnectReady)
            {
                // show error on login modal and stop if WalletConnectModal is not ready after 10 seconds
                TDKConnectUIManager.Instance.ShowLoginModal();
                socialsErrorText.text = "Wallet Connect timed out";
                socialsErrorText.gameObject.SetActive(true);
                return;
            }
            try
            {
                await TDK.Connect.Disconnect(); // clean up any previous connection attempts
                await TDK.Connect.ConnectExternalWallet();
            }
            catch (Exception ex)
            {
                if (ex.Message != "New connection attempt has been made")
                {
                    TDKLogger.LogException($"[LoginModal:ConnectExternalWallet] Error connecting", ex);
                    if (TDKConnectUIManager.Instance.GetTransitionModal().gameObject.activeInHierarchy) // if transition modal is still open
                    {
                        // close TransitionModal, go back to login modal and show cause of error
                        TDKConnectUIManager.Instance.ShowLoginModal();
                        socialsErrorText.text = ex.Message;
                        socialsErrorText.gameObject.SetActive(true);
                    }
                }
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
