using System.Numerics;
using System.Threading.Tasks;
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using Nethereum.ABI;
using Nethereum.Hex.HexConvertors.Extensions;

#if TDK_THIRDWEB
using Thirdweb;
#endif

namespace Treasure
{
    public partial class TDK : MonoBehaviour
    {
        public static Bridgeworld Bridgeworld { get; private set; }

        private void InitBridgeworld()
        {
            Bridgeworld = new Bridgeworld();
        }
    }

    public partial class Bridgeworld
    {
        public Bridgeworld() { }

        public async Task<Harvester> GetHarvester(Contract contract)
        {
            return await TDK.API.GetHarvester(contract);
        }

        public async Task<HarvesterCorruptionRemoval> GetHarvesterCorruptionRemoval(Contract contract)
        {
            return await TDK.API.GetHarvesterCorruptionRemoval(contract);
        }
    }

    public partial class Harvester
    {
        public async Task<Transaction> ApprovePermits()
        {
            TDKLogger.Log("Approving Consumables for transfer to Harvester");
            return await TDK.Common.ApproveERC1155(Contract.Consumables, nftHandlerAddress);
        }

        public async Task<Transaction> StakePermits(int amount)
        {
            TDKLogger.Log($"Staking {amount} Permit(s) to Harvester");
            var transaction = await TDK.API.WriteTransaction(
                address: nftHandlerAddress,
                functionName: "stakeNft",
                args: new string[] { permitsAddress, permitsTokenId.ToString(), amount.ToString() }
            );
            return await TDK.Common.WaitForTransaction(transaction.queueId);
        }

        public async Task<Transaction> UnstakePermits(int amount)
        {
            TDKLogger.Log($"Unstaking {amount} Permit(s) from Harvester");
            var transaction = await TDK.API.WriteTransaction(
                address: nftHandlerAddress,
                functionName: "unstakeNft",
                args: new string[] { permitsAddress, permitsTokenId.ToString(), amount.ToString() }
            );
            return await TDK.Common.WaitForTransaction(transaction.queueId);
        }

        public async Task<Transaction> ApproveMagic(BigInteger amount)
        {
#if TDK_THIRDWEB
            TDKLogger.Log($"Approving {Utils.ToEth(amount.ToString())} MAGIC for transfer to Harvester");
            return await TDK.Common.ApproveERC20(Contract.Magic, id, amount);
#else
            TDKLogger.LogError("Unable to approve magic. TDK Identity wallet service not implemented.");
            return await Task.FromResult<Transaction>(null);
#endif
        }

        public async Task<Transaction> DepositMagic(BigInteger amount)
        {
#if TDK_THIRDWEB
            TDKLogger.Log($"Depositing {Utils.ToEth(amount.ToString())} MAGIC to Harvester");
            var chainId = await TDK.Identity.GetChainId();
            var transaction = await TDK.API.WriteTransaction(
                address: id,
                functionName: "deposit",
                args: new string[] { amount.ToString(), chainId == ChainId.ArbitrumSepolia ? "1" : "0" }
            );
            return await TDK.Common.WaitForTransaction(transaction.queueId);
#else
            TDKLogger.LogError("Unable to deposit magic. TDK Identity wallet service not implemented.");
            return await Task.FromResult<Transaction>(null);
#endif
        }

        public async Task<Transaction> WithdrawMagic(BigInteger amount)
        {
            TDKLogger.Log($"Withdrawing {Utils.ToEth(amount.ToString())} MAGIC from Harvester");
            var transaction = await TDK.API.WriteTransaction(
                address: id,
                functionName: "withdrawAmountFromAll",
                args: new string[] { amount.ToString() }
            );
            return await TDK.Common.WaitForTransaction(transaction.queueId);
        }

        public async Task<Transaction> WithdrawAllMagic()
        {
            // If user has MAGIC to claim, use the withdrawAndHarvestAll function
            if (userMagicRewardsClaimable > 0)
            {
                TDKLogger.Log($"Withdrawing and harvesting all MAGIC from Harvester");
                var transaction = await TDK.API.WriteTransaction(
                    address: id,
                    functionName: "withdrawAndHarvestAll",
                    args: new string[] { }
                );
                return await TDK.Common.WaitForTransaction(transaction.queueId);
            }

            // No MAGIC to calim, just call withdraw with full amount
            return await WithdrawMagic(userMagicStaked);
        }

        public async Task<Transaction> ClaimMagicRewards()
        {
            TDKLogger.Log($"Claiming MAGIC rewards from Harvester");
            var transaction = await TDK.API.WriteTransaction(
                address: id,
                functionName: "harvestAll",
                args: new string[] { }
            );
            return await TDK.Common.WaitForTransaction(transaction.queueId);
        }

        public async Task<Transaction> ApproveCharacters()
        {
            TDKLogger.Log("Approving characters for transfer to Harvester");
            return await TDK.Common.ApproveERC721(charactersAddress, nftHandlerAddress);
        }

        public async Task<Transaction> StakeCharacters(List<int> tokenIds)
        {
            TDKLogger.Log($"Staking {tokenIds.Count} character(s) to Harvester");
            var transaction = await TDK.API.WriteTransaction(
                address: nftHandlerAddress,
                functionName: "batchStakeNft",
                args: new object[] {
                    Enumerable.Repeat(charactersAddress, tokenIds.Count).ToArray(),
                    tokenIds.ToArray(),
                    Enumerable.Repeat(1, tokenIds.Count).ToArray(),
                 }
            );
            return await TDK.Common.WaitForTransaction(transaction.queueId);
        }

        public async Task<Transaction> UnstakeCharacters(List<int> tokenIds)
        {
            TDKLogger.Log($"Unstaking {tokenIds.Count} character(s) from Harvester");
            var transaction = await TDK.API.WriteTransaction(
                address: nftHandlerAddress,
                functionName: "batchUnstakeNft",
                args: new object[] {
                    Enumerable.Repeat(charactersAddress, tokenIds.Count).ToArray(),
                    tokenIds.ToArray(),
                    Enumerable.Repeat(1, tokenIds.Count).ToArray(),
                 }
            );
            return await TDK.Common.WaitForTransaction(transaction.queueId);
        }

        public async Task Deposit(BigInteger amount)
        {
#if TDK_THIRDWEB
            if (userMagicBalance < amount)
            {
                throw new UnityException("MAGIC balance too low");
            }

            var approvalTasks = new List<Task>();
            var stakeTasks = new List<Task>();

            var remainingDepositCap = userMagicMaxStakeable - userMagicStaked;
            if (remainingDepositCap < amount)
            {
                var capRequired = decimal.Parse(Utils.ToEth((amount - remainingDepositCap).ToString()));
                var capPerPart = decimal.Parse(Utils.ToEth(permitsMagicMaxStakeable.ToString()));
                var requiredPermits = (int)Math.Ceiling(capRequired / capPerPart);
                if (requiredPermits < userPermitsBalance)
                {
                    throw new UnityException("Ancient Permits balance too low");
                }

                if (!userPermitsApproved)
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
#else
            TDKLogger.LogError("Unable to retrieve chain ID. TDK Identity wallet service not implemented.");
            await Task.FromResult<string>(string.Empty);
#endif
        }

        public async Task<Transaction> StartCorruptionRemovals(List<CorruptionRemovalRequest> requests)
        {
            TDKLogger.Log($"Starting {requests.Count} Corruption removals");
            var args = new object[requests.Count, 3];
            for (var i = 0; i < requests.Count; i++)
            {
                var request = requests[i];
                var customData = new ABIEncode().GetABIEncoded(new ABIValue("uint256[]", request.tokenIds)).ToHex(true);
                args[i, 0] = id;
                args[i, 1] = request.recipeId;
                args[i, 2] = new string[] { customData };
            }

            var transaction = await TDK.API.WriteTransaction(
                contract: Contract.CorruptionRemoval,
                functionName: "startRemovingCorruption",
                args: new object[] { args }
            );
            return await TDK.Common.WaitForTransaction(transaction.queueId);
        }

        public async Task<Transaction> EndCorruptionRemovals(List<string> requestIds)
        {
            TDKLogger.Log("Ending corruption removals");
            var transaction = await TDK.API.WriteTransaction(
                contract: Contract.CorruptionRemoval,
                functionName: "endRemovingCorruption",
                args: new object[] { requestIds }
            );
            return await TDK.Common.WaitForTransaction(transaction.queueId);
        }
    }
}
