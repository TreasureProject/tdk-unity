using System;
using System.Numerics;
using System.Threading.Tasks;
using Thirdweb;
using TMPro;
using Treasure;
using UnityEngine;
using UnityEngine.UI;

public class HarvesterUI : MonoBehaviour
{

    public Button AuthBtn;
    public Button DepositBtn;
    public TMP_Text InfoText;

    private TDKProject _project;
    private TDKHarvesterResponse _harvesterInfo;
    private BigInteger _depositAmount = BigInteger.Parse(Utils.ToWei("1000"));
    private string _arbHarvesterAddress = "0x587dc30014e10b56907237d4880a9bf8b9518150";
    private string _arbSepoliaHarvesterAddress = "0x466d20a94e280bb419031161a6a7508438ad436f";

    void Start()
    {
        updateInfoText();
    }

    private async Task<string> GetHarvesterAddress()
    {
        var chainId = await TDK.Identity.GetChainId();
        return chainId.Equals(Chain.ArbitrumSepolia.id) ? _arbSepoliaHarvesterAddress : _arbHarvesterAddress;
    }

    private async void updateInfoText()
    {
        string smartAccountAddress = null;
        try
        {
            smartAccountAddress = await TDK.Identity.GetWalletAddress();
        }
        catch
        {

        }

        InfoText.text = $@"Smart Account: {(smartAccountAddress != null ? smartAccountAddress : '-')}

    {Utils.ToEth(_harvesterInfo.user.magicBalance.ToString())} MAGIC balance
    {_harvesterInfo.user.permitsBalance} Ancient Permits

Harvester: {_harvesterInfo.harvester.id}

    {Utils.ToEth(_harvesterInfo.user.harvesterDepositCap.ToString())} MAGIC deposit cap for smart account
    {Utils.ToEth(_harvesterInfo.user.harvesterDepositAmount.ToString())} MAGIC deposited by smart account";
    }

    public async void OnAuthBtn()
    {
        _project = await TDK.Identity.GetProject();

        if (!TDK.Identity.IsAuthenticated)
        {
            try
            {
                var token = await TDK.Identity.Authenticate(_project);
                TDKLogger.Log($"Received auth token: {token}");
                AuthBtn.GetComponentInChildren<Text>().text = "Log Out";
                DepositBtn.interactable = true;
            }
            catch (Exception e)
            {
                TDKLogger.LogError($"Error authenticating: {e}");
                return;
            }

            fetchAndUpdateHarvesterInfo();
        }
        else
        {
            TDK.Identity.LogOut();
            AuthBtn.GetComponentInChildren<Text>().text = "Authenticate";
            DepositBtn.interactable = false;
        }
    }

    private async void fetchAndUpdateHarvesterInfo()
    {
        try
        {
            var address = await GetHarvesterAddress();
            _harvesterInfo = await TDK.Harvester.GetHarvester(address);
            _harvesterInfo.harvester.id = address;
            updateInfoText();
        }
        catch (Exception e)
        {
            TDKLogger.LogError($"Error fetching Harvester info: {e}");
        }
    }

    public async void OnDepositBtn()
    {
        if (_harvesterInfo.user.magicBalance < _depositAmount)
        {
            TDKLogger.LogError($"Not enough MAGIC: {_harvesterInfo.user.magicBalance}");
            return;
        }

        bool hasDepositCapLeft = _harvesterInfo.user.harvesterDepositCap - _harvesterInfo.user.harvesterDepositAmount >= _depositAmount;
        if (!hasDepositCapLeft && _harvesterInfo.user.permitsBalance < 1)
        {
            TDKLogger.LogError("No Ancient Permits or deposit cap left");
            return;
        }

        if (!hasDepositCapLeft)
        {
            if (!_harvesterInfo.user.harvesterPermitsApproved)
            {
                TDKLogger.Log("Approving Consumables transfer...");
                await TDK.Harvester.ApproveConsumables(_harvesterInfo.harvester.nftHandlerAddress);
            }

            TDKLogger.Log("Staking Ancient Permit...");
            await TDK.Harvester.HarvesterStakeNft(
                nftHandlerAddress: _harvesterInfo.harvester.nftHandlerAddress,
                permitsAddress: _harvesterInfo.harvester.permitsAddress,
                permitsTokenId: _harvesterInfo.harvester.permitsTokenId
            );
        }

        if (_harvesterInfo.user.harvesterMagicAllowance < _depositAmount)
        {
            TDKLogger.Log("Approving MAGIC transfer...");
            await TDK.Harvester.ApproveMagic(_harvesterInfo.harvester.id, _depositAmount);
        }

        TDKLogger.Log("Depositing MAGIC...");
        await TDK.Harvester.HarvesterDepositMagic(_harvesterInfo.harvester.id, _depositAmount);

        TDKLogger.Log("Deposit complete");
        fetchAndUpdateHarvesterInfo();
    }

    public void OnRefreshBtn()
    {
        fetchAndUpdateHarvesterInfo();
    }
}
