#if TDK_THIRDWEB
using System.Numerics;
using System.Threading.Tasks;
using Thirdweb;

namespace Treasure
{
    public class TDKThirdwebService : TDKBaseService
    {
        private TDKThirdwebConfig _config;

        public override void Awake()
        {
            base.Awake();

            _config = TDK.Instance.AppConfig.GetModuleConfig<TDKThirdwebConfig>();
        }

        public async Task<BigInteger> GetChainId()
        {
            return await ThirdwebManager.Instance.SDK.wallet.GetChainId();
        }

        public async Task<string> GetAddress()
        {
            return await ThirdwebManager.Instance.SDK.wallet.GetAddress();
        }

        public async Task<string> Sign(string message)
        {
            var address = await GetAddress();
            return await ThirdwebManager.Instance.SDK.session.Request<string>("personal_sign", message, address);
        }
    }
}
#endif
