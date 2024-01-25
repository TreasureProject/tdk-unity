using System;
using Treasure;
using UnityEngine;
using UnityEngine.UI;

/**
    1. auth with Smart Account
        - create session
    2. separate connection with EOA
    3. approvals:
        - Smart Account to transfer MAGIC
        - Smart Account to transfer Ancient Permits
    4. TDK API (via Thirweb Engine)
        - transfer magic & Ancient Permits to Smart Account
        - stake from Smart Account to Harvester
        - withdraw magic & Ancient Permits to Smart Account
**/

public class ThirdwebTestUI : MonoBehaviour
{

    public Button AuthBtn;

    void Start()
    {

    }

    public async void OnAuthBtn()
    {
        TDKLogger.Log($"[ThirdwebTestUI] Authenticating...");
        try
        {
            var token = await TDK.identity.Authenticate();
            TDKLogger.Log($"[ThirdwebTestUI] Authentication successful: {token}");

            TDKLogger.Log("[ThirdwebTestUI] Fetching Harvester details...");
            var harvesterInfo = await TDK.identity.GetHarvester("0x466d20a94e280bb419031161a6a7508438ad436f");
        }
        catch (Exception e)
        {
            TDKLogger.LogError($"[ThirdwebTestUI] Authentication error: {e}");
        }
    }
}
