#if TDK_THIRDWEB
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

            // var sdk = ThirdwebManager.Instance.SDK;
        }
    }
}
#endif
