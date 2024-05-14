using System;
using System.Numerics;
using System.Threading.Tasks;

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
    public Button WithdrawBtn;
    public Button StakeCharactersBtn;
    public Button UnstakeCharactersBtn;
    public TMP_Text InfoText;

    private Harvester _harvester;
    private HarvesterCorruptionRemoval _harvesterCorruptionRemoval;
    private BigInteger _magicAmount = BigInteger.Parse("1000000000000000000000"); // 1000 MAGIC

    void Start()
    {
        refreshHarvester();
    }

    private async void refreshHarvester()
    {
#if TDK_THIRDWEB
        var harvesterTask = TDK.Bridgeworld.GetHarvester(Treasure.Contract.HarvesterEmberwing);
        var harvesterCorruptionRemovalTask = TDK.Bridgeworld.GetHarvesterCorruptionRemoval(Treasure.Contract.HarvesterEmberwing);

        await Task.WhenAll(harvesterTask, harvesterCorruptionRemovalTask);

        _harvester = harvesterTask.Result;
        _harvesterCorruptionRemoval = harvesterCorruptionRemovalTask.Result;

        string smartAccountAddress = null;
        if (TDK.Identity.IsAuthenticated)
        {
            smartAccountAddress = TDK.Identity.Address;
        }

        InfoText.text = $@"
Smart Account: {(smartAccountAddress != null ? smartAccountAddress : '-')}

    {Utils.ToEth(_harvester.userMagicBalance.ToString())} MAGIC balance
    {_harvester.userPermitsBalance} Ancient Permits

Harvester Details

    Mining Power: {_harvester.totalBoost}X
    Corruption: {Utils.ToEth(_harvester.totalCorruption.ToString())} / {Utils.ToEth(_harvester.corruptionMaxGenerated.ToString())}
    {Utils.ToEth(_harvester.totalMagicStaked.ToString())} MAGIC staked

Corruption Removal

    {_harvesterCorruptionRemoval.corruptionRemovalRecipes.Count} recipe(s) available

Harvester User Details

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
                var project = await TDK.API.GetProjectBySlug(TDK.Instance.AppConfig.CartridgeTag);
                var token = await TDK.Identity.StartUserSession(project);
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
            TDK.Identity.EndUserSession();
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
