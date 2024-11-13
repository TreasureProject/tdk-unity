using System.Numerics;
using System.Threading.Tasks;
using Thirdweb;
using UnityEngine;

namespace Treasure
{
    public partial class TDK : MonoBehaviour
    {
        public static Common Common { get; private set; }

        private void InitCommon()
        {
            Common = new Common();
        }
    }

    public class Common
    {
        public Common() { }

        public string GetContractAddress(Contract contract, ChainId chainId = ChainId.Unknown)
        {
            return Constants.ContractAddresses[chainId == ChainId.Unknown ? TDK.Connect.ChainId : chainId][contract];
        }

        public async Task<Transaction> WaitForTransaction(string queueId, int maxRetries = 15, int retryMs = 2500, int initialWaitMs = 4000)
        {
            var retries = 0;
            Transaction transaction;
            do
            {
                // equivalent to `await Task.Delay();` but works on webgl
                var sw = new System.Diagnostics.Stopwatch();
                sw.Start();
                while (sw.ElapsedMilliseconds < (retries == 0 ? initialWaitMs : retryMs))
                {
                    await Task.Yield();
                }
                sw.Stop();

                transaction = await TDK.API.GetTransactionByQueueId(queueId);
                retries++;
            } while (
                retries < maxRetries &&
                transaction.status != "errored" &&
                transaction.status != "cancelled" &&
                transaction.status != "mined"
            );

            if (transaction.status == "errored")
            {
                throw new UnityException($"[WaitForTransaction] Transaction {queueId} errored: {transaction.errorMessage}");
            }

            if (transaction.status == "cancelled")
            {
                throw new UnityException($"[WaitForTransaction] Transaction {queueId} cancelled");
            }

            if (transaction.status != "mined")
            {
                throw new UnityException($"[WaitForTransaction] Transaction {queueId} timed out with status: {transaction.status}");
            }

            return transaction;
        }

        public async Task<ThirdwebContract> GetContract(string address)
        {
            var client = TDKServiceLocator.GetService<TDKThirdwebService>().Client;
            return await ThirdwebContract.Create(client, address, TDK.Connect.ChainIdNumber);
        }

        public async Task<Transaction> ApproveERC20(string address, string operatorAddress, BigInteger amount)
        {
            var transaction = await TDK.API.WriteTransaction(
                address: address,
                functionName: "approve",
                args: new string[] { operatorAddress, amount.ToString() }
            );
            return transaction;
        }

        public async Task<Transaction> ApproveERC1155(string address, string operatorAddress)
        {
            var transaction = await TDK.API.WriteTransaction(
                address: address,
                functionName: "setApprovalForAll",
                args: new string[] { operatorAddress, "true" }
            );
            return transaction;
        }

        public async Task<Transaction> ApproveERC721(string address, string operatorAddress)
        {
            var transaction = await TDK.API.WriteTransaction(
                address: address,
                functionName: "setApprovalForAll",
                args: new string[] { operatorAddress, "true" }
            );
            return transaction;
        }

        public async Task<BigInteger> GetERC20Balance(string tokenAddress, string address)
        {
            var contract = await GetContract(tokenAddress);
            return await contract.ERC20_BalanceOf(address);
        }

        public async Task<string> GetFormattedERC20Balance(string tokenAddress, string address)
        {
            var contract = await GetContract(tokenAddress);
            var decimalsTask = contract.ERC20_Decimals();
            var balanceTask = contract.ERC20_BalanceOf(address);
            await Task.WhenAll(decimalsTask, balanceTask);
            return Utils.FormatERC20(balanceTask.Result.ToString(), 4, decimalsTask.Result);
        }

        public async Task<string> GetFormattedERC20Balance(string tokenAddress, string address, int decimals)
        {
            var balance = await GetERC20Balance(tokenAddress, address);
            return Utils.FormatERC20(balance.ToString(), 4, decimals);
        }
    }
}
