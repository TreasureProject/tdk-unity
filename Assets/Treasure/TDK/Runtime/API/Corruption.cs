using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;

namespace Treasure
{
    [Serializable]
    public struct CorruptionRemovalRecipe
    {
        public struct Item
        {
            public string address;
            public List<int> tokenIds;
            public BigInteger amount;
            public string customHandler;
        }

        public string id;
        public BigInteger corruptionRemoved;
        public List<Item> items;
    }

    public struct CorruptionRemovalRequest
    {
        public string recipeId;
        public BigInteger[] tokenIds;
    }

    public struct CorruptionRemoval
    {
        public string requestId;
        // TODO: change this to enum? Started | Ready
        public string status;
    }

    [Serializable]
    public struct HarvesterCorruptionRemoval
    {
        public List<CorruptionRemovalRecipe> corruptionRemovalRecipes;
        public List<InventoryToken> userInventoryCorruptionRemovalRecipeItems;
        public List<CorruptionRemoval> userCorruptionRemovals;
    }

    public partial class API
    {
        public async Task<HarvesterCorruptionRemoval> GetHarvesterCorruptionRemoval(string id)
        {
            var response = await Get($"/harvesters/{id}/corruption-removal");
            return JsonConvert.DeserializeObject<HarvesterCorruptionRemoval>(response);
        }
    }
}
