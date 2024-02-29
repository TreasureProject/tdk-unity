#if TDK_THIRDWEB
using System.Threading.Tasks;
using Thirdweb;

namespace Treasure
{
    public class TDKThirdwebService : TDKBaseService
    {
        private TDKThirdwebConfig _config;

        public Wallet Wallet
        {
            get { return ThirdwebManager.Instance.SDK.wallet; }
        }

        public override void Awake()
        {
            base.Awake();

            _config = TDK.Instance.AppConfig.GetModuleConfig<TDKThirdwebConfig>();
        }

        public async Task<string> Sign(string message)
        {
            return await ThirdwebManager.Instance.SDK.wallet.Sign(message);
        }
    }
}
#endif
