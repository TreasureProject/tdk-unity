using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;

namespace Treasure
{
    [Serializable]
    public partial class Harvester
    {
        public struct Booster
        {
            public BigInteger tokenId;
            public string user;
            public int endTimestamp;
        }

        public string id;
        public string nftHandlerAddress;
        public string permitsAddress;
        public int permitsTokenId;
        public BigInteger permitsDepositCap;
        public string boostersStakingRulesAddress;
        public int boostersMaxStakeable;
        public List<Booster> boosters;
        public BigInteger boostersTotalBoost;
        public BigInteger userMagicBalance;
        public BigInteger userMagicAllowance;
        public int userPermitsBalance;
        public bool userPermitsApproved;
        public List<(int, int)> userBoostersBalances;
        public bool userBoostersApproved;
        public BigInteger userDepositCap;
        public BigInteger userDepositAmount;
    }

    public partial class API
    {
        public async Task<Harvester> GetHarvesterById(string id)
        {
            var response = await Get($"/harvesters/{id}");
            return JsonConvert.DeserializeObject<Harvester>(response);
        }

        public async Task<Harvester> GetHarvester(Contract contract)
        {
            var chainId = await TDK.Identity.GetChainId();
            return await GetHarvesterById(Constants.ContractAddresses[chainId][contract]);
        }
    }
}
