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

            confirmCode.onClick.AddListener(() =>
            {
                _enteredCode = codeInput.text;
                if (!IsDigitsOnlyRegex(_enteredCode) || _enteredCode.Length != 6)
                {
                    errorText.text = "Please enter 6 digits code";
                    errorText.gameObject.SetActive(true);
                    return;
                }
                errorText.gameObject.SetActive(false);
                StartCoroutine(WaitToConfirmLogin());
            });

            /* foreach (var (input, index) in codeInputs.WithIndex())
             {
                 input.onValidateInput += delegate (string s, int i, char c)
                 {
                     if (i > 0) return default;
                     if (!char.IsDigit(c)) return default;

                     var array = _enteredCode.ToCharArray();
                     array[index] = c;
                     _enteredCode = new string(array);

                     return c;
                 };

                 input.onValueChanged.AddListener(value =>
                 {
                     var direction = string.IsNullOrEmpty(value) ? 0 : 1;

                     if (index >= codeInputs.Count - 1 && direction == 1) return;

                     if (direction == 0)
                     {
                         var array = _enteredCode.ToCharArray();
                         if (!char.IsDigit(array[index]))
                             direction = -1;

                         array[index] = 'a';
                         _enteredCode = new string(array);
                     }

                     if (direction == -1 && index == 0) return;
                     codeInputs[index + direction].Select();
                 });

                 input.onSelect.AddListener(value => SetKeyboardSpace(true));
                 input.onDeselect.AddListener(value => SetKeyboardSpace(false));
             }*/

            codeInput.onSelect.AddListener(value => SetKeyboardSpace(true));
            codeInput.onDeselect.AddListener(value => SetKeyboardSpace(false));
        }

        private void SetupFromSettings()
        {
            confirmationInputCodeHolder.SetActive(appSettingsData.hasCodeToConfirmEmail);
            infoText.text = appSettingsData.hasCodeToConfirmEmail ?
                "We’ve sent a code to your email, please enter it below to confirm your login" :
                "Please click the link in the email to verify your login";
        }

        // test code
        IEnumerator WaitToConfirmLogin()
        {
            confirmCode.GetComponent<LoadingButton>().SetLoading(true);
            yield return new WaitForSeconds(2.5f);
            confirmCode.GetComponent<LoadingButton>().SetLoading(false);
            UIManager.Instance.ShowLoggedInView();
        }

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
