using UnityEngine;
using Treasure;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Networking;

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

    // TODO move this
    public string GetAuthToken() {
        var args = System.Environment.GetCommandLineArgs();
        foreach (var arg in args)
        {
            if (arg.StartsWith("--tdk-auth-token"))
            {
                var splitArg = arg.Split("=");
                if (splitArg.Length == 2)
                {
                    return splitArg[1];
                }
            }
        }
        return null;
    }

    public async void OnConnectWithAuthTokenBtn() {
        try
        {
            // get auth token
            var authToken = GetAuthToken();
            // authToken = "eyJhbGciOiJFUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiIweDE5MWMwNTBCNzJEOTk1NTMyN0RlODUyY2IyQzEyZDE2MDM3Q2EzRkQiLCJzdWIiOiIweDk3MjZjNEQzMUJlNTY2YjFEMTJkY0M2NTgzMjE3OEM4NzUzRDRiMjkiLCJhdWQiOiJsb2dpbi5zcGVsbGNhc3Rlci5sb2wiLCJleHAiOjE3MjU1NjAwNjcsIm5iZiI6MTcyNTQ3MzA2NSwiaWF0IjoxNzI1NDczNjY3LCJqdGkiOiIweDM1OWNlNmRhOTM2ZDE0YzlhMGM5NjA1MTU1MzA3MGJhOGQ2YWJmYjZkNjI3NTA4OGUyZTBlZDE2ZjkyNGRiNjAiLCJjdHgiOnsiaWQiOiJjbTBldHcxbW0wMDJoY3l3Z3RmNXc1M25lIiwiYWRkcmVzcyI6IjB4OTcyNmM0ZDMxYmU1NjZiMWQxMmRjYzY1ODMyMTc4Yzg3NTNkNGIyOSIsImVtYWlsIjoiZmFyY2hpQHRyZWFzdXJlLmxvbCIsInNtYXJ0QWNjb3VudEFkZHJlc3MiOiIweDk3MjZjNGQzMWJlNTY2YjFkMTJkY2M2NTgzMjE3OGM4NzUzZDRiMjkifX0.MHg0YWU3NDI2ZjhlY2IyNmZmNWIzMDAyZmFmNDBjNDIxNzcxZmQ0OTM4ZDgxZTI0Y2E3OTdlMmFmYTJmOTYyOGFkNDU2ZGZmZDhkODFmMDAxYTc2N2IzNjU3MGMzM2FkYWI2Zjc5NWMyM2NjZmM2OTQ4ZTFkNjdkZWNhOTM5MTViYTFj";
            if (authToken == null) {
                TDKLogger.LogInfo("No auth token found!");
                return;
            }
            TDKLogger.LogInfo(authToken);

            // start session
            var backendWallet = await TDK.AppConfig.GetBackendWallet();
            var callTargets = await TDK.AppConfig.GetCallTargets();
            var nativeTokenLimitPerTransaction = await TDK.AppConfig.GetNativeTokenLimitPerTransaction();
            var sessionDurationSec = TDK.AppConfig.SessionDurationSec;
            var sessionMinDurationLeftSec = TDK.AppConfig.SessionMinDurationLeftSec;

            TDKLogger.LogInfo(backendWallet);
            TDKLogger.LogInfo(callTargets.ToList().ToString());
            TDKLogger.LogInfo(nativeTokenLimitPerTransaction.ToString());
            TDKLogger.LogInfo(sessionDurationSec.ToString());
            TDKLogger.LogInfo(sessionMinDurationLeftSec.ToString());

            var body = Newtonsoft.Json.JsonConvert.SerializeObject(new {
                backendWallet,
                approvedTargets = callTargets,
                nativeTokenLimitPerTransaction,
                sessionDurationSec,
                sessionMinDurationLeftSec,
            });

            TDKLogger.LogInfo(body);

            using (UnityWebRequest www = UnityWebRequest.Post("http://localhost:16001/tdk-start-session", body, "application/json"))
            {
                await www.SendWebRequest();

                var rawResponse = www.downloadHandler.text;

                if (www.result != UnityWebRequest.Result.Success)
                {
                    throw new UnityException($"ConnectWithAuth error - {www.error}: {rawResponse}");
                }
                else
                {
                    TDKLogger.LogInfo(rawResponse);
                }
            }

            // validate auth token
            var chainId = await TDK.Connect.GetChainId();
            await TDK.Identity.ValidateUserSession(chainId, authToken);
        }
        catch (System.Exception ex)
        {
            TDKLogger.LogException("Error connecting with auth token", ex);
        }
    }
}
