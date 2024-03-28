using System.Threading.Tasks;
using Thirdweb;

namespace Treasure
{
    public class TDKThirdwebService : TDKBaseService
    {
#if TDK_THIRDWEB
        private TDKThirdwebConfig _config;

        public Wallet Wallet
        {
            get { return ThirdwebManager.Instance.SDK.Wallet; }
        }

        public override void Awake()
        {
            base.Awake();

            _config = TDK.Instance.AppConfig.GetModuleConfig<TDKThirdwebConfig>();
        }

        public async Task<string> Sign(string message)
        {
            return await ThirdwebManager.Instance.SDK.Wallet.Sign(message);
        }
#endif
    }
}
