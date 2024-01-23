#if TDK_THIRDWEB
using System.Threading.Tasks;
using Thirdweb;

namespace Treasure
{
    public class TDKThirdwebService : TDKBaseService
    {
        private TDKThirdwebConfig _config;

        private string _authToken;

        public override void Awake()
        {
            base.Awake();

            _config = TDK.Instance.AppConfig.GetModuleConfig<TDKThirdwebConfig>();

            // var sdk = ThirdwebManager.Instance.SDK;
        }

        public string AuthToken
        {
            get { return _authToken; }
        }

        public async Task<string> GetAddress()
        {
            return await ThirdwebManager.Instance.SDK.wallet.GetAddress();
        }
    }
}
#endif
