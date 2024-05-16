using UnityEngine;
using Treasure;
using System.Collections.Generic;

public class ConnectUI : MonoBehaviour
{
    [SerializeField] private DropDownPopUp dropdownDialogPrefab;

    private List<string> _chainIdentifiers = new List<string> {
        "arbitrum",
        "arbitrum-sepolia",
        "ethereum",
        "sepolia",
        "treasure-ruby"
    };
    private List<ChainId> _chainIds = new List<ChainId> {
        ChainId.Arbitrum,
        ChainId.ArbitrumSepolia,
        ChainId.Mainnet,
        ChainId.Sepolia,
        ChainId.TreasureRuby
    };

    public void OnConnectWalletBtn()
    {
        TDK.Connect.ShowConnectModal();
    }

    public void OnSetChainBtn()
    {
        Instantiate(dropdownDialogPrefab, transform.GetComponentInParent<Canvas>().transform)
            .Show("Set Chain", "Select an option from one of the chains below:", OnChainDropdownSubmit, _chainIdentifiers);
    }

    public async void OnChainDropdownSubmit(int value)
    {
        await TDK.Connect.SetChainId(_chainIds[value]);
    }
}
