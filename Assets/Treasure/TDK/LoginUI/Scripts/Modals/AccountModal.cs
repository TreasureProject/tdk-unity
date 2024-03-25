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

        private void Start()
        {
            closeButton.onClick.AddListener(() =>
            {
                Hide();
                TDKIdentityUIManager.Instance.HideUI();
            });
            profileButton.onClick.AddListener(() =>
            {

            });
            disconnectButton.onClick.AddListener(() =>
            {
                TDKIdentityUIManager.Instance.LogOut();
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
