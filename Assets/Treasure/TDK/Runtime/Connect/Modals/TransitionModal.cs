using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Treasure
{
    public class TransitionModal : ModalBase
    {
        [Space]
        [SerializeField] private TMP_Text headerText;
        [SerializeField] private TMP_Text infoText;
        [SerializeField] private Button actionButton;
        [SerializeField] private TMP_Text actionButtonText;

        public void SetInfoLabels(string header, string info)
        {
            headerText.text = header;
            infoText.text = info;
        }

        public void SetButtonAction(string text, Action buttonAction)
        {
            actionButtonText.text = text;
            actionButton.onClick.RemoveAllListeners();
            actionButton.onClick.AddListener(() =>
            {
                actionButton.onClick.RemoveAllListeners();
                buttonAction();
            });
        }
    }
}
