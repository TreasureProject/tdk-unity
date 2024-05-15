using UnityEngine;
using Treasure;

public class ConnectUI : MonoBehaviour
{
    public void OnConnectWalletBtn()
    {
        TDK.Connect.ShowConnectModal();
    }

    public async void OnSetChainBtn()
    {
        // TODO: Use a dropdown to select chain
        if (await TDK.Connect.GetChainId() == ChainId.ArbitrumSepolia)
        {
            await TDK.Connect.SetChainId(ChainId.Sepolia);
        }
        else
        {
            await TDK.Connect.SetChainId(ChainId.ArbitrumSepolia);
        }
    }
}
