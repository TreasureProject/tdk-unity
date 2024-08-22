using System.Numerics;
using System.Threading.Tasks;
using Thirdweb;

using TMPro;
using Treasure;
using UnityEngine;
using UnityEngine.UI;

public class HarvesterUI : MonoBehaviour
{
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

        DepositBtn.interactable = _harvester.userMagicBalance >= _magicAmount;
        WithdrawBtn.interactable = _harvester.userMagicStaked >= _magicAmount;
        StakeCharactersBtn.interactable = _harvester.userInventoryCharacters != null && _harvester.userInventoryCharacters.Count > 0;
        UnstakeCharactersBtn.interactable = _harvester.userCharactersStaked > 0;
    }

    public async void OnDepositBtn()
    {
        await _harvester.Deposit(_magicAmount);
        refreshHarvester();
    }

    public async void OnWithdrawBtn()
    {
        await _harvester.WithdrawMagic(_magicAmount);
        refreshHarvester();
    }

    public async void OnStakeCharactersBtn()
    {
        await _harvester.StakeCharacters(_harvester.userInventoryCharacters.ConvertAll(token => token.tokenId));
        refreshHarvester();
    }

    public async void OnUnstakeCharactersBtn()
    {
        await _harvester.UnstakeCharacters(_harvester.userStakedCharacters.ConvertAll(token => token.tokenId));
        refreshHarvester();
    }

    public void OnRefreshBtn()
    {
        refreshHarvester();
    }
}
