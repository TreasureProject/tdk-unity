using UnityEngine;
using Treasure;

public class IdentityUI : MonoBehaviour
{
    public void OnTreasureConnectBtn()
    {
        TDK.Connect.ShowConnectModal();
    }

    public void OnPrintWalletAddressBtn()
    {
        TDKLogger.Log(TDK.Connect.Address);
    }
}
