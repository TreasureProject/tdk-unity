using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

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

        private Action OnOkAction;

        private void Start()
        {
            okButton.onClick.AddListener(() =>
            {
                OnOkAction?.Invoke();
                Debug.Log(inputField.text);
                Hide();
            });
            cancelButton.onClick.AddListener(() =>
            {
                Hide();
            });
        }

        public void Show(string title, string description, Action onOkAction)
        {
            titleText.text = title;
            descriptionText.text = description;
            OnOkAction = onOkAction;
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
