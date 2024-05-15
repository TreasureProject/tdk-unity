using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Treasure
{
    public class InputPopUp : MonoBehaviour
    {
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text descriptionText;
        [Space]
        [SerializeField] private TMP_InputField inputField;
        [Space]
        [SerializeField] private Button okButton;
        [SerializeField] private Button cancelButton;

        private Action<string> OnOkAction;

        private void Start()
        {
            okButton.onClick.AddListener(() =>
            {
                OnOkAction?.Invoke(inputField.text);
                Hide();
            });
            cancelButton.onClick.AddListener(() =>
            {
                Hide();
            });
        }

        public void Show(string title, string description, Action<string> onOkAction)
        {
            titleText.text = title;
            descriptionText.text = description;
            OnOkAction = onOkAction;
        }

        public void Hide()
        {
            Destroy(this.gameObject);
        }
    }
}
