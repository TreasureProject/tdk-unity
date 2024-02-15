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
            TDKLogger.Log("Approving Consumables for transfer to Harvester");
            return await TDK.Common.ApproveERC1155(Contract.Consumables, nftHandlerAddress);
        }

        private async Task<Transaction> StakePermits(int amount)
        {
            TDKLogger.Log($"Staking {amount} Permit(s) to Harvester");
            var transaction = await TDK.API.WriteTransaction(
                address: nftHandlerAddress,
                functionName: "stakeNft",
                args: new string[] { permitsAddress, permitsTokenId.ToString(), amount.ToString() }
            );
            return await TDK.Common.WaitForTransaction(transaction.queueId);
        }

        private async Task<Transaction> ApproveMagic(BigInteger amount)
        {
            TDKLogger.Log($"Approving {Thirdweb.Utils.ToEth(amount.ToString())} MAGIC for transfer to Harvester");
            return await TDK.Common.ApproveERC20(Contract.Magic, id, amount);
        }

        private async Task<Transaction> DepositMagic(BigInteger amount)
        {
            TDKLogger.Log($"Depositing {Thirdweb.Utils.ToEth(amount.ToString())} MAGIC to Harvester");
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
                var capRequired = decimal.Parse(Thirdweb.Utils.ToEth((amount - remainingDepositCap).ToString()));
                var capPerPart = decimal.Parse(Thirdweb.Utils.ToEth(permitsDepositCap.ToString()));
                var requiredPermits = (int)Math.Ceiling(capRequired / capPerPart);
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
