using System;
using System.Numerics;

#if TDK_THIRDWEB
using Thirdweb;
#endif

using TMPro;
using Treasure;
using UnityEngine;
using UnityEngine.UI;

public class HarvesterUI : MonoBehaviour
{

    public Button AuthBtn;
    public Button DepositBtn;
    public TMP_Text InfoText;

    private Harvester _harvester;

    void Start()
    {
        refreshHarvester();
    }

    private async void refreshHarvester()
    {
#if TDK_THIRDWEB
        _harvester = await TDK.Bridgeworld.GetHarvester(Treasure.Contract.HarvesterEmberwing);
        string smartAccountAddress = null;
        if (TDK.Identity.IsAuthenticated)
        {
            smartAccountAddress = await TDK.Identity.GetWalletAddress();
        }

        InfoText.text = $@"Smart Account: {(smartAccountAddress != null ? smartAccountAddress : '-')}

    {Utils.ToEth(_harvester.userMagicBalance.ToString())} MAGIC balance
    {_harvester.userPermitsBalance} Ancient Permits

Harvester: {_harvester.id}

    {Utils.ToEth(_harvester.userMagicMaxStakeable.ToString())} MAGIC max stakeable for smart account
    {Utils.ToEth(_harvester.userMagicStaked.ToString())} MAGIC staked by smart account";
#else
        await Task.FromResult<string>(string.Empty);
#endif
    }

    public async void OnAuthBtn()
    {
        if (!TDK.Identity.IsAuthenticated)
        {
            try
            {
                var token = await TDK.Identity.Authenticate(TDK.Instance.AppConfig.CartridgeTag);
                TDKLogger.Log($"Received auth token: {token}");
                AuthBtn.GetComponentInChildren<Text>().text = "Log Out";
                DepositBtn.interactable = true;
                refreshHarvester();
            }
            catch (Exception e)
            {
                TDKLogger.LogError($"Error authenticating: {e}");
                return;
            }
        }
        else
        {
            TDK.Identity.LogOut();
            AuthBtn.GetComponentInChildren<Text>().text = "Authenticate";
            DepositBtn.interactable = false;
        }
    }

    public async void OnDepositBtn()
    {
#if TDK_THIRDWEB
        await _harvester.Deposit(BigInteger.Parse(Utils.ToWei("1000")));
        refreshHarvester();
#else
        await Task.FromResult<string>(string.Empty);
#endif
    }

    public void OnRefreshBtn()
    {
        refreshHarvester();
    }
}
