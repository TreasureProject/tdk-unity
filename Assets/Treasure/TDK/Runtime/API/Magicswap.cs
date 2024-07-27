using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Treasure
{
    [Serializable]
    public class MagicswapToken {
        [Serializable]
        public class VaultCollection {
            [Serializable]
            public class VaultCollectionData {
                public string id;
                public string type;
            }

            public VaultCollectionData collection;
            public List<string> tokenIds;
        }

        [Serializable]
        public class Collection {
            public string id;
            public string urlSlug;
            public List<string> tokenIds;
            public string name;
            public string symbol;
            public string type;
            public string image;
        }

        public string id;
        public string name;
        public string symbol;
        public int decimals;
        public string derivedMAGIC;
        public bool isNFT;
        public List<VaultCollection> vaultCollections;
        public string type;
        public string image;
        public bool isMAGIC;
        public List<Collection> collections;
        public string urlSlug;
        public string collectionId;
        public List<string> collectionTokenIds;
        public double priceUSD;
        public string reserve;
    }

    
    
    [Serializable]
    public class MagicswapPool {
        [Serializable]
        public class DayData {
            public string reserveUSD;
            public string volumeUSD;
            public string txCount;
        }

        public string id;
        public MagicswapToken token0;
        public MagicswapToken token1;
        public string reserve0;
        public string reserve1;
        public BigInteger reserveUSD; // TODO check type; big, int or double? examples are 0
        public string totalSupply;
        public string txCount;
        public double volumeUSD;
        public string lpFee;
        public string protocolFee;
        public string royaltiesFee;
        public string royaltiesBeneficiary = null;
        public string totalFee;
        public List<DayData> dayData; // TODO confirm naming
        public string name;
        public bool hasNFT;
        public bool isNFTNFT;
        public double volume24h0;
        public double volume1w0;
        public double volume24h1;
        public double volume1w1;
    }

    [Serializable]
    public class MagicswapRoute {
        [Serializable]
        public class RouteLeg {
            public class LegTokenInfo {
                public string name;
                public string symbol;
                public string address;
                public int decimals;
                public string tokenId;
            }

            public string poolAddress;
            public string poolType;
            public double poolFee;
            public LegTokenInfo tokenFrom;
            public LegTokenInfo tokenTo;
            public BigInteger assumedAmountIn;
            public BigInteger assumedAmountOut;
            public int swapPortion; // TODO check type; examples are 1
            public int absolutePortion; // TODO check type; examples are 1
        }

        public string amountIn;
        public string amountOut;
        public MagicswapToken tokenIn;
        public MagicswapToken tokenOut;
        public List<RouteLeg> legs;
        public List<string> path;
        public double priceImpact;
        public int derivedValue; // TODO check type; examples are small-ish ints
        public double lpFee;
        public double protocolFee;
        public double royaltiesFee;
    }

    [Serializable]
    public class MagicswapPoolsResponse {
        public List<MagicswapPool> pools;
    }

    [Serializable]
    public class SwapBody {
        [Serializable]
        public class NFTInput {
            public string id;
            public int quantity; // TODO check type
        }

        public string backendWallet;
        public string tokenInId;
        public string tokenOutId;
        public string amountIn;
        public string amountOut;
        public List<string> path;
        public bool isExactOut;
        public List<NFTInput> nftsIn;
        public List<NFTInput> nftsOut;
        public int slippage; // TODO check type
    }

    public partial class API
    {
        public async Task<List<MagicswapPool>> GetAllPools()
        {
            var response = await Get("/magicswap/pools");
            return JsonConvert.DeserializeObject<MagicswapPoolsResponse>(response).pools;
        }
        
        public async Task<MagicswapPool> GetPoolById(string id)
        {
            var response = await Get($"/magicswap/pools/{id}");
            return JsonConvert.DeserializeObject<MagicswapPool>(response);
        }

        public async Task<MagicswapRoute> GetRoute(string tokenInId, string tokenOutId, string amount, bool isExactOut)
        {
            var body = JsonConvert.SerializeObject(new { tokenInId, tokenOutId, amount, isExactOut });
            var response = await Post("/magicswap/route", body);
            return JsonConvert.DeserializeObject<MagicswapRoute>(response);
        }

        // TODO add to TDK harness scene
        public async Task<Transaction> Swap(SwapBody swapBody) {
            var body = JsonConvert.SerializeObject(swapBody);
            var response = await Post("/magicswap/swap", body);
            var transaction = JsonConvert.DeserializeObject<Transaction>(response);
            transaction = await TDK.Common.WaitForTransaction(transaction.queueId);
            return transaction;
        }
    }
}
