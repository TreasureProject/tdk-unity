using UnityEngine;
using System;

namespace Treasure
{
    public class TDKThirdwebConfig : ScriptableObject
    {
        [SerializeField] private string _clientId;
        [SerializeField] private string _factoryAddress;
        [SerializeField] private bool _gasless;
        [SerializeField] private string _bundlerUrl;
        [SerializeField] private string _paymasterUrl;
        [SerializeField] private string _entryPointAddress;

        public string ClientId
        {
            get { return _clientId; }
        }

        public string FactoryAddress
        {
            get { return _factoryAddress; }
        }

        public bool Gasless
        {
            get { return _gasless; }
        }

        public string BundlerUrl
        {
            get { return _bundlerUrl; }
        }

        public string PaymasterUrl
        {
            get { return _paymasterUrl; }
        }

        public string EntryPointAddress
        {
            get { return _entryPointAddress; }
        }

        public void SetConfig(SerializedThirdwebConfig config)
        {
            _clientId = config.clientId;
            _factoryAddress = config.factoryAddress;
            _gasless = config.gasless;
            _bundlerUrl = config.bundlerUrl;
            _paymasterUrl = config.paymasterUrl;
            _entryPointAddress = config.entryPointAddress;
        }
    }

    [Serializable]
    public class SerializedThirdwebConfig
    {
        [SerializeField] public string clientId;
        [SerializeField] public string factoryAddress;
        [SerializeField] public bool gasless;
        [SerializeField] public string bundlerUrl;
        [SerializeField] public string paymasterUrl;
        [SerializeField] public string entryPointAddress;
    }
}
