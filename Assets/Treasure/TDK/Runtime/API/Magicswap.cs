using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
        public bool isETH;
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
        public BigInteger reserveUSD;
        public string totalSupply;
        public string txCount;
        public double volumeUSD;
        public string lpFee;
        public string protocolFee;
        public string royaltiesFee;
        public string royaltiesBeneficiary = null;
        public string totalFee;
        public List<DayData> dayData;
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
        }

        public string amountIn;
        public string amountOut;
        public MagicswapToken tokenIn;
        public MagicswapToken tokenOut;
        public List<RouteLeg> legs;
        public List<string> path;
        public double priceImpact;
        public double derivedValue;
        public double lpFee;
        public double protocolFee;
        public double royaltiesFee;
    }

    [Serializable]
    public class MagicswapPoolsResponse {
        public List<MagicswapPool> pools;
    }

    [Serializable]
    public class NFTInput {
        public string id;
        public int quantity;
    }

    [Serializable]
    public class SwapBody {
        public string tokenInId;
        public string tokenOutId;
        public string amountIn;
        public string amountOut;
        public List<string> path;
        public bool isExactOut = false;
        public List<NFTInput> nftsIn = null;
        public List<NFTInput> nftsOut = null;
        public double? slippage = null;
        public string backendWallet = null;
        public string toAddress = null;
    }

    [Serializable]
    public class AddLiquidityBody {
        public List<NFTInput> nfts0 = null;
        public List<NFTInput> nfts1 = null;
        public string amount0;
        public string amount1;
        public string amount0Min;
        public string amount1Min;
        public string backendWallet = null;
        public string toAddress = null;
    }

    [Serializable]
    public class RemoveLiquidityBody {
        public List<NFTInput> nfts0 = null;
        public List<NFTInput> nfts1 = null;
        public string amountLP;
        public string amount0Min;
        public string amount1Min;
        public bool swapLeftover = true;
        public string backendWallet = null;
        public string toAddress = null;
    }

    [Serializable]
    public class ContractCallArgs {
        public string address;
        public string functionName;
        public object[] args;
        public string value;

        public WriteTransactionBody ToWriteTransactionBody() {
            for (int i = 0; i < args.Length; i++) 
            {
                if (args[i] is JArray jArray)
                {
                    var subArray = jArray.ToObject<object[]>();
                    args[i] = subArray;
                    for (int j = 0; j < subArray.Length; j++)
                    {
                        if (subArray[j] is string subStringArg)
                        {
                            if (!subStringArg.StartsWith("0x"))
                            {
                                var successfullyConverted = BigInteger.TryParse(subStringArg, out BigInteger result);
                                if (successfullyConverted)
                                {
                                    subArray[j] = result;
                                }
                            }
                        }
                    }
                }
                if (args[i] is string stringArg)
                {
                    if (!stringArg.StartsWith("0x"))
                    {
                        var successfullyConverted = BigInteger.TryParse(stringArg, out BigInteger result);
                        if (successfullyConverted)
                        {
                            args[i] = result;
                        }
                    }
                }

            }
            return new WriteTransactionBody {
                address = address,
                args = args,
                functionName = functionName,
                txOverrides = new WriteTransactionBody.TransactionOverrides {
                    value = value
                },
                abi = Abis.magicswapRouterV2,
            };
        }
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

        public async Task<Transaction> Swap(SwapBody swapBody) {
            swapBody.backendWallet ??= TDK.AppConfig.GetBackendWallet();
            swapBody.toAddress ??= TDK.Identity.Address;
            var body = JsonConvert.SerializeObject(swapBody, new JsonSerializerSettings {
                NullValueHandling = NullValueHandling.Ignore
            });
            var thirdwebService = TDKServiceLocator.GetService<TDKThirdwebService>();
            if (await thirdwebService.IsZkSyncChain(TDK.Connect.ChainIdNumber))
            {
                var argsResponse = await Post("/magicswap/swap/args", body);
                var contractCallArgs = JsonConvert.DeserializeObject<ContractCallArgs>(argsResponse);
                return await thirdwebService.WriteTransaction(contractCallArgs.ToWriteTransactionBody());
            }
            var response = await Post("/magicswap/swap", body);
            var transaction = JsonConvert.DeserializeObject<Transaction>(response);
            transaction = await TDK.Common.WaitForTransaction(transaction.queueId);
            return transaction;
        }

        public async Task<Transaction> AddLiquidity(string poolId, AddLiquidityBody addLiquidityBody) {
            addLiquidityBody.backendWallet ??= TDK.AppConfig.GetBackendWallet();
            addLiquidityBody.toAddress ??= TDK.Identity.Address;
            var body = JsonConvert.SerializeObject(addLiquidityBody, new JsonSerializerSettings {
                NullValueHandling = NullValueHandling.Ignore
            });
            var thirdwebService = TDKServiceLocator.GetService<TDKThirdwebService>();
            if (await thirdwebService.IsZkSyncChain(TDK.Connect.ChainIdNumber))
            {
                var argsResponse = await Post($"/magicswap/pools/{poolId}/add-liquidity/args", body);
                var contractCallArgs = JsonConvert.DeserializeObject<ContractCallArgs>(argsResponse);
                return await thirdwebService.WriteTransaction(contractCallArgs.ToWriteTransactionBody());
            }
            var response = await Post($"/magicswap/pools/{poolId}/add-liquidity", body);
            var transaction = JsonConvert.DeserializeObject<Transaction>(response);
            transaction = await TDK.Common.WaitForTransaction(transaction.queueId);
            return transaction;
        }

        public async Task<Transaction> RemoveLiquidity(string poolId, RemoveLiquidityBody removeLiquidityBody) {
            removeLiquidityBody.backendWallet ??= TDK.AppConfig.GetBackendWallet();
            removeLiquidityBody.toAddress ??= TDK.Identity.Address;
            var body = JsonConvert.SerializeObject(removeLiquidityBody, new JsonSerializerSettings {
                NullValueHandling = NullValueHandling.Ignore
            });
            var thirdwebService = TDKServiceLocator.GetService<TDKThirdwebService>();
            if (await thirdwebService.IsZkSyncChain(TDK.Connect.ChainIdNumber))
            {
                var argsResponse = await Post($"/magicswap/pools/{poolId}/remove-liquidity/args", body);
                var contractCallArgs = JsonConvert.DeserializeObject<ContractCallArgs>(argsResponse);
                return await thirdwebService.WriteTransaction(contractCallArgs.ToWriteTransactionBody());
            }
            var response = await Post($"/magicswap/pools/{poolId}/remove-liquidity", body);
            var transaction = JsonConvert.DeserializeObject<Transaction>(response);
            transaction = await TDK.Common.WaitForTransaction(transaction.queueId);
            return transaction;
        }
    }
}
