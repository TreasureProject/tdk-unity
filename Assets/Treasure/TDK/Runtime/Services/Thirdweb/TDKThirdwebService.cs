#if TDK_THIRDWEB
using UnityEngine;
using UnityEngine.Events;
using Unity.Android.Types;
using System;
using System.Threading.Tasks;
using Thirdweb;

using System.Numerics;

namespace Treasure
{
    public class TDKThirdwebService : TDKBaseService
    {
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
    }
}
#endif
