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
        public string permitsStakingRulesAddress;
        public string boostersStakingRulesAddress;
        public string legionsStakingRulesAddress;
        public string treasuresStakingRulesAddress;
        public string charactersStakingRulesAddress;
        public string charactersAddress;
        public string permitsAddress;
        public int permitsTokenId;
        public int permitsMaxStakeable;
        public BigInteger permitsMagicMaxStakeable;
        public int boostersMaxStakeable;
        public BigInteger magicMaxStakeable;
        public BigInteger corruptionMaxGenerated;
        public double totalEmissionsActivated;
        public BigInteger totalMagicStaked;
        public double totalBoost;
        public double totalBoostersBoost;
        public BigInteger totalCorruption;
        public List<Booster> boosters;
        public BigInteger userMagicBalance;
        public BigInteger userMagicAllowance;
        public int userPermitsBalance;
        public bool userPermitsApproved;
        public Dictionary<int, int> userBoostersBalances;
        public bool userBoostersApproved;
        public double userTotalBoost;
        public int userPermitsMaxStakeable;
        public int userPermitsStaked;
        public List<InventoryToken> userInventoryCharacters;
        public List<Token> userStakedCharacters;
        public bool userCharactersApproved;
        public int userCharactersMaxStakeable;
        public int userCharactersStaked;
        public double userCharactersMaxBoost;
        public double userCharactersBoost;
        public BigInteger userMagicMaxStakeable;
        public BigInteger userMagicStaked;
        public BigInteger userMagicRewardsClaimable;
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
