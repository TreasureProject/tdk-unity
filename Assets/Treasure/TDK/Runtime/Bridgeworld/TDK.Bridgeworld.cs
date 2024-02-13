using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using UnityEngine;

namespace Treasure
{
    public partial class TDK : MonoBehaviour
    {
        public static Bridgeworld Bridgeworld;

        private void InitBridgeworld()
        {
            Bridgeworld = new Bridgeworld();
        }
    }

    public class Bridgeworld
    {
        public Bridgeworld() { }

        public async Task<Harvester> GetHarvester(Contract contract)
        {
            return await TDK.API.GetHarvester(contract);
        }
    }

    public partial class Harvester
    {
        private async Task<Transaction> ApprovePermits()
        {
            return await TDK.Common.ApproveERC1155(Contract.Consumables, nftHandlerAddress);
        }

        private async Task<Transaction> StakePermits(int amount)
        {
            var transaction = await TDK.API.WriteTransaction(
                address: nftHandlerAddress,
                functionName: "stakeNft",
                args: new string[] { permitsAddress, permitsTokenId.ToString(), amount.ToString() }
            );
            return await TDK.Common.WaitForTransaction(transaction.queueId);
        }

        private async Task<Transaction> ApproveMagic(BigInteger amount)
        {
            return await TDK.Common.ApproveERC20(Contract.Magic, id, amount);
        }

        private async Task<Transaction> DepositMagic(BigInteger amount)
        {
            var chainId = await TDK.Identity.GetChainId();
            var transaction = await TDK.API.WriteTransaction(
                address: id,
                functionName: "deposit",
                args: new string[] { amount.ToString(), chainId == ChainId.ArbitrumSepolia ? "1" : "0" }
            );
            return await TDK.Common.WaitForTransaction(transaction.queueId);
        }

        public async Task Deposit(BigInteger amount)
        {
            if (userMagicBalance < amount)
            {
                throw new UnityException("MAGIC balance too low");
            }

            var approvalTasks = new List<Task>();
            var stakeTasks = new List<Task>();

            var remainingDepositCap = userDepositCap - userDepositAmount;
            if (remainingDepositCap < amount)
            {
                var requiredPermits = (int)Math.Ceiling(decimal.Parse(Thirdweb.Utils.ToEth((amount - remainingDepositCap / permitDepositCap).ToString())));
                if (requiredPermits < userPermitsBalance)
                {
                    throw new UnityException("Ancient Permits balance too low");
                }

                if (!userApprovedPermits)
                {
                    approvalTasks.Add(ApprovePermits());
                }

                stakeTasks.Add(StakePermits(requiredPermits));
            }

            if (userMagicAllowance < amount)
            {
                approvalTasks.Add(ApproveMagic(amount));
            }

            await Task.WhenAll(approvalTasks);
            await Task.WhenAll(stakeTasks);
            await DepositMagic(amount);
        }
    }
}
