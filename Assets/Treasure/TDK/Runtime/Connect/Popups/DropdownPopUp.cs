using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Treasure
{
    public class DropDownPopUp : MonoBehaviour
    {
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text descriptionText;
        [Space]
        [SerializeField] private TMP_Dropdown dropdown;
        [Space]
        [SerializeField] private Button okButton;
        [SerializeField] private Button cancelButton;

        private Action<int> OnOkAction;

        private void Start()
        {
            okButton.onClick.AddListener(() =>
            {
                OnOkAction?.Invoke(dropdown.value);
                Hide();
            });
            cancelButton.onClick.AddListener(() =>
            {
                Hide();
            });
        }

        public void Show(string title, string description, Action<int> onOkAction, List<string> dropDownOptions)
        {
            titleText.text = title;
            descriptionText.text = description;
            OnOkAction = onOkAction;

            dropdown.ClearOptions();
            for (int i = 0; i < dropDownOptions.Count; i++)
            {
                var option = new TMP_Dropdown.OptionData();
                option.text = dropDownOptions[i];
                dropdown.options.Add(option);
            }
        }

        public void Hide()
        {
            Destroy(this.gameObject);
        }
    }
}
