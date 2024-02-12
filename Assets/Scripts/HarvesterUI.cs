using System;
using System.Numerics;
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

    void Start()
    {
        updateInfoText();
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

Harvester: {TDK.Instance.AppConfig.HarvesterAddress}

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
            _harvesterInfo = await TDK.Harvester.GetHarvester(TDK.Instance.AppConfig.HarvesterAddress);
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
            await TDK.Harvester.ApproveMagic(TDK.Instance.AppConfig.HarvesterAddress, _depositAmount);
        }

        TDKLogger.Log("Depositing MAGIC...");
        await TDK.Harvester.HarvesterDepositMagic(TDK.Instance.AppConfig.HarvesterAddress, _depositAmount);

        TDKLogger.Log("Deposit complete");
        fetchAndUpdateHarvesterInfo();
    }

    public void OnRefreshBtn()
    {
        fetchAndUpdateHarvesterInfo();
    }
}
