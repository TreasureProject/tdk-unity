using System.Collections.Generic;
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

    public partial class Magicswap
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
    }
}
