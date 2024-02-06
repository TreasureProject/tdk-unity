using System;
using System.Numerics;
using System.Threading.Tasks;
using Thirdweb;
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
    public Button DepositBtn;

    private string _harvesterAddress = "0x466d20a94e280bb419031161a6a7508438ad436f";
    private TDKProject _project;
    private TDKHarvesterResponse _harvesterInfo;
    private BigInteger _depositAmount = BigInteger.Parse(Utils.ToWei("1000"));

    void Start()
    {

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
            }

            // Immediately fetch Harvester info so we can display status to user
            try
            {
                _harvesterInfo = await TDK.Identity.GetHarvester(_harvesterAddress);
            }
            catch (Exception e)
            {
                TDKLogger.LogError($"Error fetching Harvester info: {e}");
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
                await TDK.Identity.ApproveConsumables(_harvesterInfo.harvester.nftHandlerAddress);
                await Task.Delay(20_000);
            }

            TDKLogger.Log("Staking Ancient Permit...");
            await TDK.Identity.HarvesterStakeNft(
                nftHandlerAddress: _harvesterInfo.harvester.nftHandlerAddress,
                permitsAddress: _harvesterInfo.harvester.permitsAddress,
                permitsTokenId: _harvesterInfo.harvester.permitsTokenId
            );
            await Task.Delay(20_000);
        }

        if (_harvesterInfo.user.harvesterMagicAllowance < _depositAmount)
        {
            TDKLogger.Log("Approving MAGIC transfer...");
            await TDK.Identity.ApproveMagic(_harvesterAddress, _depositAmount);
            await Task.Delay(20_000);
        }

        TDKLogger.Log("Depositing MAGIC...");
        await TDK.Identity.HarvesterDepositMagic(_harvesterAddress, _depositAmount);
    }
}
