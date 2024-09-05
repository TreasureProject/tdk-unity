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
        catch (Exception e)
        {
            // TODO improve error handling generally:
            // - doing `Debug.LogError(e)` gives a better error trace
            // - not catching at all makes the stack trace "file + line links" clickable
            // - could try TDKLogger.LogException here
            TDKLogger.LogError($"Error starting user session: {e.Message}");
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

    public async void OnAuthTokenSubmit(string value)
    {
        try
        {
            var chainId = await TDK.Connect.GetChainId();
            await TDK.Identity.ValidateUserSession(chainId, value);
        }
        catch (Exception e)
        {
            TDKLogger.LogError($"Error validating user session: {e.Message}");
        }
    }

    public async void OnGetMagicBalanceBtn()
    {
        var contractAddress = await TDK.Common.GetContractAddress(Treasure.Contract.Magic);
        var contract = TDKServiceLocator.GetService<TDKThirdwebService>().SDK.GetContract(contractAddress);
        var balance = await contract.Read<string>("balanceOf", TDK.Identity.Address);
        TDKLogger.LogInfo($"Magic balance: {Utils.ToEth(balance)}");
    }

    public async void OnMintMagicBtn()
    {
        TDKLogger.LogInfo("Minting 1,000 MAGIC...");
        var transaction = await TDK.API.WriteTransaction(Treasure.Contract.Magic, "mint", new object[] { TDK.Identity.Address, Utils.ToWei("1000") });
        transaction = await TDK.Common.WaitForTransaction(transaction.queueId);
        if (transaction.status == "errored")
        {
            TDKLogger.LogError($"Mint failed: {transaction.errorMessage}");
        }
        else
        {
            TDKLogger.LogInfo($"Mint successful: {transaction.transactionHash}");
        }
    }

    public async void OnRawMintMagicBtn()
    {
        TDKLogger.LogInfo("Minting 1,000 MAGIC...");
        var contractAddress = await TDK.Common.GetContractAddress(Treasure.Contract.Magic);
        var contract = TDKServiceLocator.GetService<TDKThirdwebService>().SDK.GetContract(contractAddress);
        var tx = await contract.Prepare(
            functionName: "mint",
            args: new object[] { TDK.Identity.Address, BigInteger.Parse(Utils.ToWei("1000")) }
        );
        await tx.Populate();
        var transaction = await TDK.API.SendRawTransaction(new SendRawTransactionBody()
        {
            to = contract.Address,
            data = tx.Input.Data,
        });
        transaction = await TDK.Common.WaitForTransaction(transaction.queueId);
        if (transaction.status == "errored")
        {
            TDKLogger.LogError($"Mint failed: {transaction.errorMessage}");
        }
        else
        {
            TDKLogger.LogInfo($"Mint successful: {transaction.transactionHash}");
        }
    }

    public async void OnSendEthBtn()
    {
        TDKLogger.LogInfo("Sending 0.0001 ETH to 0xe647b2c46365741e85268ced243113d08f7e00b8...");
        var transaction = await TDK.API.SendRawTransaction(new SendRawTransactionBody()
        {
            to = "0xe647b2c46365741e85268ced243113d08f7e00b8",
            value = Utils.ToWei("0.0001"),
            data = "0x",
        });
        transaction = await TDK.Common.WaitForTransaction(transaction.queueId);
        if (transaction.status == "errored")
        {
            TDKLogger.LogError($"Send failed: {transaction.errorMessage}");
        }
        else
        {
            TDKLogger.LogInfo($"Send successful: {transaction.transactionHash}");
        }
    }
}
