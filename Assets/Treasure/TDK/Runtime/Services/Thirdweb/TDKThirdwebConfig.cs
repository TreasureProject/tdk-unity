using UnityEngine;
using System;

namespace Treasure
{
    public class TDKThirdwebConfig : ScriptableObject
    {
        [SerializeField] private string _devClientId;
        [SerializeField] private string _prodClientId;
        [SerializeField] private string _devDefaultChainIdentifier = "arbitrum-sepolia";
        [SerializeField] private string _prodDefaultChainIdentifier = "arbitrum";
        [SerializeField] private string _factoryAddress;

        public string ClientId
        {
            get { return TDK.Instance.AppConfig.Environment == TDKConfig.Env.DEV ? _devClientId : _prodClientId; }
        }

        public string DefaultChainIdentifier
        {
            get { return TDK.Instance.AppConfig.Environment == TDKConfig.Env.DEV ? _devDefaultChainIdentifier : _prodDefaultChainIdentifier; }
        }

        public string FactoryAddress
        {
            get { return _factoryAddress; }
        }

        public void SetConfig(SerializedThirdwebConfig config)
        {
            _devClientId = config.devClientId;
            _prodClientId = config.prodClientId;
            _devDefaultChainIdentifier = config.devDefaultChainIdentifier;
            _prodDefaultChainIdentifier = config.prodDefaultChainIdentifier;
            _factoryAddress = config.factoryAddress;
        }
    }

    [Serializable]
    public class SerializedThirdwebConfig
    {
        [SerializeField] public string devClientId;
        [SerializeField] public string prodClientId;
        [SerializeField] public string devDefaultChainIdentifier;
        [SerializeField] public string prodDefaultChainIdentifier;
        [SerializeField] public string factoryAddress;
    }
}
