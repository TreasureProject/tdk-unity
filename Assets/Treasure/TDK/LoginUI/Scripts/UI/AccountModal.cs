using UnityEngine;
using UnityEngine.UI;

namespace Treasure
{
    public class AccountModal : ModalBase
    {
        [SerializeField] private Button disconnectButton;
        [SerializeField] private Button profileButton;

        private void Start()
        {
            profileButton.onClick.AddListener(() =>
            {

            });
            disconnectButton.onClick.AddListener(() =>
            {
                UIManager.Instance.LogOut();
            });
        }
    }
}
