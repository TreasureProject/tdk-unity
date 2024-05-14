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

            ThirdwebManager.Instance.clientId = _config.ClientId;
            ThirdwebManager.Instance.factoryAddress = Constants.ContractAddresses[(ChainId)_config.DefaultChainId][Contract.ManagedAccountFactory];
            ThirdwebManager.Instance.gasless = true;
            ThirdwebManager.Instance.Initialize(_config.DefaultChainId.ToString());
        }
    }
}
#endif
