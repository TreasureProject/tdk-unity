using Newtonsoft.Json;
using System;
using System.Numerics;
using System.Threading.Tasks;

namespace Treasure
{
    [Serializable]
    public partial class Harvester
    {
        public string id;
        public string nftHandlerAddress;
        public string permitsAddress;
        public int permitsTokenId;
        public BigInteger permitsDepositCap;
        public BigInteger userMagicBalance;
        public int userPermitsBalance;
        public BigInteger userMagicAllowance;
        public bool userApprovedPermits;
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
