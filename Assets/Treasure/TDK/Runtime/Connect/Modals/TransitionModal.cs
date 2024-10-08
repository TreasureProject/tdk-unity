using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Treasure
{
    // TODO reword in a way that prompts user to take action ("Sign into your account in the pop-up/browser"?)
    public class TransitionModal : ModalBase
    {
        [Space]
        [SerializeField] private Button cancelButton;

        public void SetCancelAction(Action onCancel)
        {
            cancelButton.onClick.RemoveAllListeners();
            cancelButton.onClick.AddListener(() =>
            {
                cancelButton.onClick.RemoveAllListeners();
                onCancel();
            });
        }
    }
}
