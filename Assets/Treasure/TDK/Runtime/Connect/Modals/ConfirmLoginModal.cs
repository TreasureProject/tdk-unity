using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text.RegularExpressions;

namespace Treasure
{
    public class ConfirmLoginModal : ModalBase
    {
        [SerializeField] private TMP_Text infoText;
        [SerializeField] private GameObject confirmationInputCodeHolder;
        [Space]
        [SerializeField] private Button confirmCode;
        [SerializeField] private Button didntReceiveEmailButton;
        [Space]
        [SerializeField] private TMP_InputField codeInput;
        [SerializeField] private TMP_Text errorText;
        [SerializeField] private LayoutElement keyBoardSpace;

        private string _email;
        private string _enteredCode = "aaaaaa";

        private void Start()
        {
            codeInput.onSelect.AddListener(value => SetKeyboardSpace(true));
            codeInput.onDeselect.AddListener(value => SetKeyboardSpace(false));

            didntReceiveEmailButton.onClick.AddListener(() =>
            {
                _ = TDK.Connect.Disconnect();
                TDKConnectUIManager.Instance.ShowLoginModal();
            });
        }

        private void OnEnable()
        {
            confirmCode.GetComponent<LoadingButton>().SetLoading(false);
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

        private void SetKeyboardSpace(bool value)
        {
            keyBoardSpace.gameObject.SetActive(value);
        }

        private bool IsDigitsOnlyRegex(string str)
        {
            return Regex.IsMatch(str, "^[0-9]+$");
        }
    }
}
