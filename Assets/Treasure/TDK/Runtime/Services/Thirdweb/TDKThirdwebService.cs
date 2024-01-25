#if TDK_THIRDWEB
using System.Threading.Tasks;
using Thirdweb;

namespace Treasure
{
    public class TDKThirdwebService : TDKBaseService
    {
        private TDKThirdwebConfig _config;

        public Wallet wallet
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
            return await ThirdwebManager.Instance.SDK.session.Request<string>("personal_sign", message, await wallet.GetAddress());
        }
    }
}
#endif
