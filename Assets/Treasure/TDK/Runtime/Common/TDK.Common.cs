using System.Numerics;
using System.Threading.Tasks;
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

        public async Task<Transaction> WaitForTransaction(string queueId)
        {
            var retries = 0;
            Transaction transaction;
            do
            {
                if (retries > 0)
                {
                    await Task.Delay(2_500);
                }

                transaction = await TDK.API.GetTransactionByQueueId(queueId);
                retries++;
            } while (
                retries < 15 &&
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

        public async Task<Transaction> ApproveERC20(Contract contract, string operatorAddress, BigInteger amount)
        {
            var transaction = await TDK.API.WriteTransaction(
                contract: contract,
                functionName: "approve",
                args: new string[] { operatorAddress, amount.ToString() }
            );
            return await WaitForTransaction(transaction.queueId);
        }

        public async Task<Transaction> ApproveERC1155(Contract contract, string operatorAddress)
        {
            var transaction = await TDK.API.WriteTransaction(
                contract: contract,
                functionName: "setApprovalForAll",
                args: new string[] { operatorAddress, "true" }
            );
            return await WaitForTransaction(transaction.queueId);
        }
    }
}
