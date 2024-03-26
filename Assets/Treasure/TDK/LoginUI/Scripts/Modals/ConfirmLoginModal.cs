using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text.RegularExpressions;
using UnityEngine.Windows;

namespace Treasure
{
    public class ConfirmLoginModal : ModalBase
    {
        [SerializeField] private AppSettingsData appSettingsData;
        [SerializeField] private TMP_Text infoText;
        [SerializeField] private GameObject confirmationInputCodeHolder;
        [Space]
        [SerializeField] private Button confirmCode;
        [SerializeField] private Button didntReceiveEmailButton;
        [Space]
        [SerializeField] private TMP_InputField codeInput;
        [SerializeField] private TMP_Text errorText;
        [SerializeField] private LayoutElement keyBoardSpace;

        private string _enteredCode = "aaaaaa";

        private void Start()
        {
            SetupFromSettings();

            codeInput.onSelect.AddListener(value => SetKeyboardSpace(true));
            codeInput.onDeselect.AddListener(value => SetKeyboardSpace(false));

            didntReceiveEmailButton.onClick.AddListener(() =>
            {
                TDKIdentityUIManager.Instance.ShowLoginModal(true);
            });
        }

        private void OnEnable()
        {
            confirmCode.GetComponent<LoadingButton>().SetLoading(false);

            infoText.text = appSettingsData.hasCodeToConfirmEmail ?
                $"We have sent a code to {TDKIdentityUIManager.Instance.GetUserEmail()}, please enter it below to confirm  your login" :
                "Please click the link in the email to verify your login";
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

        public  void SetErrorText(string text)
        {
            errorText.text = text;
            errorText.gameObject.SetActive(true);
        }

        private void SetupFromSettings()
        {
            confirmationInputCodeHolder.SetActive(appSettingsData.hasCodeToConfirmEmail);
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
