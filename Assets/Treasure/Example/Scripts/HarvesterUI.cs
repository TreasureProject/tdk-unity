using System;
using System.Numerics;

#if TDK_THIRDWEB
using Thirdweb;
#else
using System.Threading.Tasks;
#endif

using TMPro;
using Treasure;
using UnityEngine;
using UnityEngine.UI;

public class HarvesterUI : MonoBehaviour
{

    public Button AuthBtn;
    public Button DepositBtn;
    public Button WithdrawBtn;
    public Button StakeCharactersBtn;
    public Button UnstakeCharactersBtn;
    public TMP_Text InfoText;

    private Harvester _harvester;
    private BigInteger _magicAmount = BigInteger.Parse(Utils.ToWei("1000"));

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

    Global Details
    Mining Power: {_harvester.totalBoost}X
    Corruption: {Utils.ToEth(_harvester.totalCorruption.ToString())} / {Utils.ToEth(_harvester.corruptionMaxGenerated.ToString())}
    {Utils.ToEth(_harvester.totalMagicStaked.ToString())} MAGIC staked

    User Details
    Mining Power: {_harvester.userTotalBoost}X
    {_harvester.userPermitsStaked} Ancient Permits staked
    {Utils.ToEth(_harvester.userMagicMaxStakeable.ToString())} MAGIC max stakeable
    {Utils.ToEth(_harvester.userMagicStaked.ToString())} MAGIC staked
    {Utils.ToEth(_harvester.userMagicRewardsClaimable.ToString())} MAGIC rewards claimable
    {_harvester.userCharactersStaked} character(s) staked";
#else
        await Task.FromResult<string>(string.Empty);
#endif

        DepositBtn.interactable = _harvester.userMagicBalance >= _magicAmount;
        WithdrawBtn.interactable = _harvester.userMagicStaked >= _magicAmount;
        StakeCharactersBtn.interactable = _harvester.userInventoryCharacters != null && _harvester.userInventoryCharacters.Count > 0;
        UnstakeCharactersBtn.interactable = _harvester.userCharactersStaked > 0;
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
            refreshHarvester();
        }
    }

    public async void OnDepositBtn()
    {
#if TDK_THIRDWEB
        await _harvester.Deposit(_magicAmount);
        refreshHarvester();
#else
        await Task.FromResult<string>(string.Empty);
#endif
    }

    public async void OnWithdrawBtn()
    {
#if TDK_THIRDWEB
        await _harvester.WithdrawMagic(_magicAmount);
        refreshHarvester();
#else
        await Task.FromResult<string>(string.Empty);
#endif
    }

    public async void OnStakeCharactersBtn()
    {
#if TDK_THIRDWEB
        await _harvester.StakeCharacters(_harvester.userInventoryCharacters.ConvertAll(token => token.tokenId));
        refreshHarvester();
#else
        await Task.FromResult<string>(string.Empty);
#endif
    }

    public async void OnUnstakeCharactersBtn()
    {
#if TDK_THIRDWEB
        await _harvester.UnstakeCharacters(_harvester.userStakedCharacters.ConvertAll(token => token.tokenId));
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
