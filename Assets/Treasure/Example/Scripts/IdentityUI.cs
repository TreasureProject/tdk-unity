using UnityEngine;
using Treasure;

public class IdentityUI : MonoBehaviour
{
    public async void OnPrintWalletAddressBtn()
    {
        var walletAddr = await TDK.Identity.GetWalletAddress();

        Debug.Log("Wallet address is:" + walletAddr);
    }

    public void OnConnectBtn()
    {
        UIManager.Instance.ShowLoginModal();
    }

    public void OnAccountBtn()
    {
        UIManager.Instance.ShowAccountModal();
    }
}
