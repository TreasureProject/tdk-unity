using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Treasure
{
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
