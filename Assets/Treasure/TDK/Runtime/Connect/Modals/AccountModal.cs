using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Treasure
{
    public class AccountModal : ModalBase
    {
        [Space]
        [SerializeField] private Button closeButton;
        [Space]
        [SerializeField] private Button disconnectButton;
        [SerializeField] private Button profileButton;
        [Space]
        [SerializeField] private TMP_Text adressText;
        [SerializeField] private CopyButton copyButton;

        private async void Start()
        {
            closeButton.onClick.AddListener(() =>
            {
                Hide();
                TDKIdentityUIManager.Instance.Hide();
            });
            profileButton.onClick.AddListener(() =>
            {

            });
            disconnectButton.onClick.AddListener(() =>
            {
                TDKIdentityUIManager.Instance.LogOut();
            });

            SetAddress(await TDK.Identity.GetWalletAddress());
        }

        public void SetAddress(string address)
        {
            adressText.text = $"{address[..6]}...{address.Substring(address.Length - 4)}";
            copyButton.SetTextToCopy(address);
        }
    }
}
