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
                TDKConnectUIManager.Instance.Hide();
            });
            profileButton.onClick.AddListener(() =>
            {

            });
            disconnectButton.onClick.AddListener(() =>
            {
                TDKConnectUIManager.Instance.LogOut();
                _ = TDK.Identity.EndUserSession();
            });
        }

        private void OnEnable()
        {
            SetAddress(TDK.Connect.Address);
        }

        public void SetAddress(string address)
        {
            adressText.text = $"{address[..6]}...{address.Substring(address.Length - 4)}";
            copyButton.SetTextToCopy(address);
        }

        public string GetAddressText() {
            return adressText.text;
        }
    }
}
