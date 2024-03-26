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
        
        private bool useSmartWallets = true;
        private ChainData _currentChainData;
        private string _address;
        private string _email;

        public UnityEvent<WalletConnection> onConnectionRequested = new UnityEvent<WalletConnection>();
        public UnityEvent<Exception> onConnectionError = new UnityEvent<Exception>();
        public UnityEvent<string> onConnected = new UnityEvent<string>();

        public Wallet Wallet
        {
            get { return ThirdwebManager.Instance.SDK.wallet; }
        }

        public override void Awake()
        {
            base.Awake();

            _config = TDK.Instance.AppConfig.GetModuleConfig<TDKThirdwebConfig>();
        }

        public void Start()
        {
            _currentChainData = ThirdwebManager.Instance.supportedChains.Find(x => x.identifier == ThirdwebManager.Instance.activeChain);
        }

        public async Task<string> Sign(string message)
        {
            return await ThirdwebManager.Instance.SDK.wallet.Sign(message);
        }


    }
}
#endif
