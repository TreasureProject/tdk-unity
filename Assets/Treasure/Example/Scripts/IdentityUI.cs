using System;
using System.Numerics;
using Thirdweb;
using Treasure;
using UnityEngine;

public class IdentityUI : MonoBehaviour
{
    [SerializeField] private InputPopUp promptDialogPrefab;

    public async void OnStartUserSessionBtn()
    {
        try
        {
            _ = await TDK.Identity.StartUserSession();
        }
        catch (Exception ex)
        {
            TDKLogger.LogException($"Error starting user session", ex);
        }
    }

    public void OnEndUserSessionBtn()
    {
        _ = TDK.Identity.EndUserSession();
    }

    public void OnValidateUserSessionBtn()
    {
        Instantiate(promptDialogPrefab, transform.GetComponentInParent<Canvas>().transform)
            .Show("Validate User Session", "Enter an auth token below to validate:", OnAuthTokenSubmit);
    }

    public async void OnStartUserSessionLauncherBtn()
    {
        try
        {
            await TDK.Identity.StartUserSessionViaLauncher();
        }
        catch (Exception ex)
        {
            TDKLogger.LogException($"Error starting user session", ex);
        }
    }

    public async void OnAuthTokenSubmit(string value)
    {
        try
        {
            await TDK.Identity.ValidateUserSession(TDK.Connect.ChainId, value);
        }
        catch (Exception e)
        {
            TDKLogger.LogError($"Error validating user session: {e.Message}");
        }
    }

    public async void OnGetMagicBalanceBtn()
    {
        var thirdwebService = TDKServiceLocator.GetService<TDKThirdwebService>();
        var usingZkSyncChain = await thirdwebService.IsZkSyncChain(TDK.Connect.ChainIdNumber);
        if (usingZkSyncChain)
        {
            var balance = await TDK.Common.GetFormattedNativeBalance();
            TDKLogger.LogInfo($"Magic balance: {balance}");
        }
        else
        {
            var contractAddress = TDK.Common.GetContractAddress(Contract.Magic);
            var balance = await TDK.Common.GetFormattedERC20Balance(contractAddress, TDK.Identity.Address);
            TDKLogger.LogInfo($"Magic balance: {balance}");
        }
    }

    public async void OnMintMagicBtn()
    {
        TDKLogger.LogInfo("Minting 1,000 MAGIC...");
        var contractAddress = TDK.Common.GetContractAddress(Contract.Magic);
        try
        {
            var args = new object[] { TDK.Identity.Address, Utils.ToWei("1000") };
            
            var estimate = await TDK.API.EstimateTransactionGas(contractAddress, "mint", args);
            TDKLogger.LogInfo($"Estimated gas cost: {estimate.Ether}");

            var transaction = await TDK.API.WriteTransaction(contractAddress, "mint", args);
            TDKLogger.LogInfo($"Mint successful: {transaction.transactionHash}");
        }
        catch (Exception ex)
        {
            TDKLogger.LogException("Mint failed", ex);
        }
    }

    public async void OnRawMintMagicBtn()
    {
        TDKLogger.LogInfo("Minting 1,000 MAGIC...");
        var contractAddress = TDK.Common.GetContractAddress(Contract.Magic);
        var thirdwebService = TDKServiceLocator.GetService<TDKThirdwebService>();
        var contract = await ThirdwebContract.Create(thirdwebService.Client, contractAddress, TDK.Connect.ChainIdNumber);
        var tx = await ThirdwebContract.Prepare(
            wallet: thirdwebService.ActiveWallet,
            contract: contract,
            method: "mint",
            weiValue: 0,
            // method params:
            TDK.Identity.Address,
            BigInteger.Parse(Utils.ToWei("1000"))
        );
        try
        {
            var transaction = await TDK.API.SendRawTransaction(new SendRawTransactionBody()
            {
                to = contractAddress,
                data = tx.Input.Data,
            });
            TDKLogger.LogInfo($"Mint successful: {transaction.transactionHash}");
        }
        catch (Exception ex)
        {
            TDKLogger.LogException("Mint failed", ex);
        }
    }

    public async void OnSendEthBtn()
    {
        TDKLogger.LogInfo("Sending 0.0001 ETH to 0xe647b2c46365741e85268ced243113d08f7e00b8...");
        try
        {
            var transaction = await TDK.API.SendRawTransaction(new SendRawTransactionBody()
            {
                to = "0xe647b2c46365741e85268ced243113d08f7e00b8",
                value = Utils.ToWei("0.0001"),
                data = "0x",
            });
            TDKLogger.LogInfo($"Send successful: {transaction.transactionHash}");
        }
        catch (Exception ex)
        {
            TDKLogger.LogException("Send failed", ex);
        }
    }
}
