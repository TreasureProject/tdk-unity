using UnityEngine;
using System;

namespace Treasure
{
    public class TDKThirdwebConfig : ScriptableObject
    {
        [SerializeField] private string _devClientId;
        [SerializeField] private string _prodClientId;
        [SerializeField] private int _devDefaultChainId = (int)ChainId.ArbitrumSepolia;
        [SerializeField] private int _prodDefaultChainId = (int)ChainId.Arbitrum;

        public string ClientId
        {
            get { return TDK.Instance.AppConfig.Environment == TDKConfig.Env.DEV ? _devClientId : _prodClientId; }
        }

        public int DefaultChainId
        {
            get { return TDK.Instance.AppConfig.Environment == TDKConfig.Env.DEV ? _devDefaultChainId : _prodDefaultChainId; }
        }

        public void SetConfig(SerializedThirdwebConfig config)
        {
            _devClientId = config.devClientId;
            _prodClientId = config.prodClientId;
            _devDefaultChainId = config.devDefaultChainId;
            _prodDefaultChainId = config.prodDefaultChainId;
        }
    }

    [Serializable]
    public class SerializedThirdwebConfig
    {
        [SerializeField] public string devClientId;
        [SerializeField] public string prodClientId;
        [SerializeField] public int devDefaultChainId;
        [SerializeField] public int prodDefaultChainId;
    }
}
