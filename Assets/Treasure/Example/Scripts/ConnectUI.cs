using UnityEngine;
using Treasure;
using System.Collections.Generic;
using UnityEngine.UI;

public class ConnectUI : MonoBehaviour
{
    [SerializeField] private DropDownPopUp dropdownDialogPrefab;
    [SerializeField] private Button webGLExternalReconnectButton;

    private List<string> _chainIdentifiers = new List<string> {
        "arbitrum",
        "arbitrum-sepolia",
        "ethereum",
        "sepolia",
        "treasure",
        "treasure-ruby",
        "treasure-topaz"
    };
    private List<ChainId> _chainIds = new List<ChainId> {
        ChainId.Arbitrum,
        ChainId.ArbitrumSepolia,
        ChainId.Mainnet,
        ChainId.Sepolia,
        ChainId.Treasure,
        ChainId.TreasureRuby,
        ChainId.TreasureTopaz
    };

    void Start()
    {
        if (TDKWebConnectInterface.IsActive())
        {
            webGLExternalReconnectButton.interactable = true;
            if (TDKWebConnectInterface.BrowserHasActiveWalletConnection())
            {
                TDKLogger.LogInfo("ConnectUI: an active wallet connection has been detected in the browser");
            }
            else
            {
                TDKWebConnectInterface.BrowserWalletConnectedAction += () =>
                {
                    if (TDKWebConnectInterface.BrowserHasActiveWalletConnection())
                    {
                        TDKLogger.LogInfo("ConnectUI: an active wallet connection has been detected in the browser");
                    }
                };
            }
        }
        else
        {
            webGLExternalReconnectButton.interactable = false;
        }
    }

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

    public async void OnWebGLExternalReconnectBtn()
    {
        var thirdwebService = TDKServiceLocator.GetService<TDKThirdwebService>();
        var isWalletConnected = await thirdwebService.IsWalletConnected();
        if (isWalletConnected)
        {
            TDKLogger.LogInfo("Wallet already connected, aborting.");
            return;
        }
        if (TDKWebConnectInterface.BrowserHasActiveWalletConnection())
        {
            TDKLogger.LogInfo("Requesting auth data from the browser...");
            TDKWebConnectInterface.AttemptReconnect();
        }
        else
        {
            var browserConnectionState = TDKWebConnectInterface.GetBrowserConnectionState();
            if (browserConnectionState == TDKWebConnectInterface.BrowserConnectionState.Disconnected)
            {
                TDKLogger.LogInfo("Invalid request: no active connection found in the browser");
            }
            else if (browserConnectionState == TDKWebConnectInterface.BrowserConnectionState.NewConnection)
            {
                TDKLogger.LogInfo("Invalid request: a new connection attempt was started");
            }
        }
    }
}
