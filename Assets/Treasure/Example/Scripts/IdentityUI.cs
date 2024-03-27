using UnityEngine;
using Treasure;

public class IdentityUI : MonoBehaviour
{
    public async void OnPrintWalletAddressBtn()
    {
        var walletAddr = await TDK.Identity.GetWalletAddress();

        TDKLogger.Log("Wallet address is:" + walletAddr);
    }

    public void OnTreasureConnectBtn()
    {
#if TDK_THIRDWEB
        TDKConnectUIManager.Instance.Show();
#endif
    }
}
