using System;
using Treasure;
using UnityEngine;

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
    void Start()
    {

    }

    public async void OnAuthBtn()
    {
        TDKLogger.Log($"[ThirdwebTestUI] Authenticating...");
        try
        {
            await TDK.identity.Authenticate();
            TDKLogger.Log($"[ThirdwebTestUI] Authentication successful");
        }
        catch (Exception e)
        {
            TDKLogger.LogError($"[ThirdwebTestUI] Authentication error: {e}");
        }
    }
}
