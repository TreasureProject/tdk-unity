using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Treasure
{
    public class AccountModal : ModalBase
    {
        [Space]
        [SerializeField] private Button disconnectButton;
        [SerializeField] private Button profileButton;
        [Space]
        [SerializeField] private TMP_Text adressText;
        [SerializeField] private CopyButton copyButton;

        private void Start()
        {
            profileButton.onClick.AddListener(() =>
            {

            });
            disconnectButton.onClick.AddListener(() =>
            {
                UIManager.Instance.LogOut();
            });

            SetAddress(TDKServiceLocator.GetService<TDKThirdwebService>().GetWalletAddress());
        }

        public void SetAddress(string address)
        {
            adressText.text = $"{address[..6]}...{address.Substring(address.Length - 4)}";
            copyButton.SetTextToCopy(address);
        }
    }
}
