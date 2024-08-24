using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using UnityEngine;

namespace Treasure
{
    public partial class TDK : MonoBehaviour
    {
        public static Magicswap Magicswap { get; private set; }

        private void InitMagicswap()
        {
            Magicswap = new Magicswap();
        }
    }

    public class Magicswap
    {
        public Magicswap() { }
        
        public async Task<List<MagicswapPool>> GetAllPools()
        {
            return await TDK.API.GetAllPools();
        }
        
        public async Task<MagicswapPool> GetPoolById(string id)
        {
            return await TDK.API.GetPoolById(id);
        }

        public async Task<MagicswapRoute> GetRoute(string tokenInId, string tokenOutId, string amount, bool isExactOut)
        {
            return await TDK.API.GetRoute(tokenInId, tokenOutId, amount, isExactOut);
        }

        public async Task<Transaction> Swap(SwapBody swapBody) {
            return await TDK.API.Swap(swapBody);
        }

        public async Task<Transaction> AddLiquidity(string poolId, AddLiquidityBody addLiquidityBody) {
            return await TDK.API.AddLiquidity(poolId, addLiquidityBody);
        }

        public async Task<Transaction> RemoveLiquidity(string poolId, RemoveLiquidityBody removeLiquidityBody) {
            return await TDK.API.RemoveLiquidity(poolId, removeLiquidityBody);
        }

        public BigInteger GetQuote(BigInteger amountA, BigInteger reserveA, BigInteger reserveB) {
            return reserveA > 0 ? amountA * reserveB / reserveA : 0;
        }

        public BigInteger GetAmountMin(BigInteger amount, double slippage) {
            return amount - amount * new BigInteger(System.Math.Ceiling(slippage * 1000)) / 1000;
        }
    }
}
