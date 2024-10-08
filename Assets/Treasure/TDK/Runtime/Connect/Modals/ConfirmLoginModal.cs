using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Thirdweb;

namespace Treasure
{
    public class ConfirmLoginModal : ModalBase
    {
        [SerializeField] private TMP_Text infoText;
        [SerializeField] private GameObject confirmationInputCodeHolder;
        [Space]
        [SerializeField] private Button confirmCode;
        [SerializeField] private Button goBackButton;
        [Space]
        [SerializeField] private TMP_InputField codeInput;
        [SerializeField] private TMP_Text errorText;
        [SerializeField] private LayoutElement keyBoardSpace;

        private string _email;
        private string _enteredCode = "aaaaaa";
        private TaskCompletionSource<bool> _currentOtpTaskCompletionSource;

        private void Start()
        {
            codeInput.onSelect.AddListener(value => SetKeyboardSpace(true));
            codeInput.onDeselect.AddListener(value => SetKeyboardSpace(false));

            goBackButton.onClick.AddListener(() =>
            {
                _ = TDK.Connect.Disconnect();
                TDKConnectUIManager.Instance.ShowLoginModal();
            });
        }

        private void OnEnable()
        {
            SetConfirmLoading(false);
            SetErrorText("");
            infoText.text = $"We have sent a code to {_email}, please enter it below to confirm your login";
        }

        public bool CheckConfirmationCodeIsValid()
        {
            _enteredCode = codeInput.text;
            if (!IsDigitsOnlyRegex(_enteredCode) || _enteredCode.Length != 6)
            {
                errorText.text = "Please enter 6 digits code";
                errorText.gameObject.SetActive(true);
                return false;
            }

            errorText.gameObject.SetActive(false);
            return true;
        }

        public void SetEmail(string email)
        {
            _email = email;
        }

        public void SetErrorText(string text)
        {
            errorText.text = text;
            errorText.gameObject.SetActive(text.Length > 0);
        }

        // test code
        /* IEnumerator WaitToConfirmLogin()
         {
             confirmCode.GetComponent<LoadingButton>().SetLoading(true);
             yield return new WaitForSeconds(2.5f);
             confirmCode.GetComponent<LoadingButton>().SetLoading(false);
             UIManager.Instance.ShowLoggedInView();
         }*/

        private void SetConfirmLoading(bool isLoading) {
            confirmCode.GetComponent<LoadingButton>().SetLoading(isLoading);
        }

        private void SetKeyboardSpace(bool value)
        {
            keyBoardSpace.gameObject.SetActive(value);
        }

        private bool IsDigitsOnlyRegex(string str)
        {
            return Regex.IsMatch(str, "^[0-9]+$");
        }

        public Task<bool> LoginWithOtp(EcosystemWallet wallet)
        {
            confirmCode.onClick.RemoveAllListeners();
            codeInput.text = string.Empty;
            codeInput.interactable = true;
            SetConfirmLoading(false);

            _currentOtpTaskCompletionSource = new TaskCompletionSource<bool>();

            confirmCode.onClick.AddListener(async () =>
            {
                try
                {
                    if (!CheckConfirmationCodeIsValid())
                    {
                        return;
                    }
                    codeInput.interactable = false;
                    SetConfirmLoading(true);
                    
                    var otp = codeInput.text;
                    var address = await wallet.LoginWithOtp(otp);
                    _currentOtpTaskCompletionSource.SetResult(true);
                }
                catch (Exception e)
                {
                    if (e.Message.Contains("try again")) {
                        SetErrorText(e.Message);
                        TDK.Analytics.TrackCustomEvent(AnalyticsConstants.EVT_TREASURECONNECT_OTP_FAILED);
                        TDKLogger.LogError("Login with OTP failed");
                        codeInput.text = string.Empty;
                        codeInput.interactable = true;
                        SetConfirmLoading(false);
                    } else {
                        TDKLogger.LogException("OTP error", e);
                        TDKConnectUIManager.Instance.ShowLoginModal();
                        _currentOtpTaskCompletionSource.SetException(e);
                    }
                }
            });

            return _currentOtpTaskCompletionSource.Task;
        }

        public void CancelCurrentLoginAttempt() {
            _currentOtpTaskCompletionSource?.TrySetException(new Exception("User closed modal"));
        }
    }
}
