using UnityEngine;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Treasure
{
    public class TDKConfig : ScriptableObject
    {
        public static string DEFAULT_CONFIG_LOCATION = "Assets/Treasure/TDK/Resources";

        [Serializable]
        public class ScriptableObjectDictionary : TreasureSerializableDictionary<string, ScriptableObject> { }

        [Serializable]
        public class SessionOption
        {
            public ChainId chainId;
            public string backendWallet;
            public List<string> callTargets;
            public double nativeTokenLimitPerTransaction;
        }

        [Serializable]
        public class GeneralConfig
        {
            public string _cartridgeTag;
            public string _cartridgeName;
            public Sprite _cartridgeIcon;
            public string _devApiUrl;
            public string _prodApiUrl;
            public string _devApiKey;
            public string _prodApiKey;
            public string _devClientId;
            public string _prodClientId;
        }

        [Serializable]
        public class ConnectConfig
        {
            public string _factoryAddress;
            public ChainId _devDefaultChainId;
            public ChainId _prodDefaultChainId;
            public int _sessionDurationSec;
            public int _sessionMinDurationLeftSec; // TODO use in ValidateActiveSigner
            public List<SessionOption> _sessionOptions;
        }

        [Serializable]
        public class AnalyticsConfig
        {
            public string _devApiUrl = "https://darkmatter.spellcaster.lol/ingress";
            public string _prodApiUrl = "https://darkmatter.treasure.lol/ingress";
        }

        public enum Env { DEV, PROD }
        public enum LoggerLevelValue { SILENT = 100, ERROR = 40, WARNING = 30, INFO = 20, DEBUG = 10 }

        [SerializeField] private Env _environment = Env.DEV;

        [SerializeField] private GeneralConfig _general;

        [SerializeField] private ConnectConfig _connect;

        [SerializeField] private AnalyticsConfig _analytics;

        [SerializeField] private ScriptableObjectDictionary moduleConfigurations = null;

        [Header("Misc")]
        [SerializeField] private LoggerLevelValue _devLoggerLevel = LoggerLevelValue.INFO;
        [SerializeField] private LoggerLevelValue _prodLoggerLevel = LoggerLevelValue.INFO;
        [SerializeField] private bool _autoInitialize = true;

        public Env Environment
        {
            get { return _environment; }
            set { _environment = value; }
        }

        public string CartridgeTag => _general._cartridgeTag;
        public string CartridgeName =>_general. _cartridgeName;
        public Sprite CartridgeIcon => _general._cartridgeIcon;

        public string TDKApiUrl => Environment == Env.DEV ? _general._devApiUrl : _general._prodApiUrl;
        public string ClientId => Environment == Env.DEV ? _general._devClientId : _general._prodClientId;

        public string FactoryAddress => _connect._factoryAddress;

        public ChainId DefaultChainId =>
            Environment == Env.DEV ? _connect._devDefaultChainId : _connect._prodDefaultChainId;

        public int SessionLengthSeconds => _connect._sessionDurationSec;

        public string AnalyticsApiUrl => Environment == Env.DEV ? _analytics._devApiUrl : _analytics._prodApiUrl;


        public LoggerLevelValue LoggerLevel
        {
            get { return Environment == Env.DEV ? _devLoggerLevel : _prodLoggerLevel; }
            set { if (Environment == Env.DEV) _devLoggerLevel = value; else _prodLoggerLevel = value; }
        }

        public string ApiKey => Environment == Env.DEV ? _general._devApiKey : _general._prodApiKey;

        public bool AutoInitialize => _autoInitialize;

        public async Task<string> GetBackendWallet()
        {
            var chainId = await TDK.Connect.GetChainId();
            var option = _connect._sessionOptions.Find(d => d.chainId == chainId);
            return option?.backendWallet.ToLowerInvariant();
        }

        public async Task<List<string>> GetCallTargets()
        {
            var chainId = await TDK.Connect.GetChainId();
            var option = _connect._sessionOptions.Find(d => d.chainId == chainId);
            return option != null ? option.callTargets.ConvertAll(ct => ct.ToLowerInvariant()) : new List<string>();
        }

        public async Task<double> GetNativeTokenLimitPerTransaction()
        {
            var chainId = await TDK.Connect.GetChainId();
            var option = _connect._sessionOptions.Find(d => d.chainId == chainId);
            return option?.nativeTokenLimitPerTransaction ?? 0;
        }

        public T GetModuleConfig<T>()
        {
            if (moduleConfigurations.ContainsKey(typeof(T).Name))
            {
                return (T)Convert.ChangeType(moduleConfigurations[typeof(T).Name], typeof(T));
            }

            return default;
        }

        public void SetModuleConfig<T>(T moduleConfig)
        {
            if (moduleConfigurations == null)
            {
                moduleConfigurations = new ScriptableObjectDictionary();
            }
            moduleConfigurations.Add(typeof(T).Name, moduleConfig);
        }

        public static TDKConfig LoadFromResources()
        {
            return Resources.Load<TDKConfig>("TDKConfig");
        }

        public void SetConfig(SerializedTDKConfig config)
        {
            // General
            _general = new GeneralConfig {
                _cartridgeTag = config.general.cartridgeTag,
                _cartridgeName = config.general.cartridgeName,
                _devApiUrl = config.general.devApiUrl,
                _prodApiUrl = config.general.prodApiUrl,
                _devApiKey = config.general.devApiKey,
                _prodApiKey = config.general.prodApiKey,
                _devClientId = config.general.devClientId,
                _prodClientId = config.general.prodClientId,
            };

            // Connect
            _connect = new ConnectConfig {
                _factoryAddress = config.connect.factoryAddress,
                _devDefaultChainId = Constants.NameToChainId.GetValueOrDefault(
                    config.connect.devDefaultChainIdentifier ?? "",
                    ChainId.Unknown
                ),
                _prodDefaultChainId = Constants.NameToChainId.GetValueOrDefault(
                    config.connect.prodDefaultChainIdentifier ?? "",
                    ChainId.Unknown
                ),
                _sessionDurationSec = config.connect.sessionDurationSec,
                _sessionMinDurationLeftSec = config.connect.sessionMinDurationLeftSec,
            };

            if (config.connect.sessionOptions != null)
            {
                _connect._sessionOptions = config.connect.sessionOptions.ConvertAll(option => new SessionOption
                {
                    chainId = Constants.NameToChainId.GetValueOrDefault(option.chainIdentifier, ChainId.Unknown),
                    backendWallet = option.backendWallet,
                    callTargets = option.callTargets,
                    nativeTokenLimitPerTransaction = option.nativeTokenLimitPerTransaction,
                });
            }
            else
            {
                _connect._sessionOptions = new List<SessionOption>();
            }

            // Analytics
            _analytics = new AnalyticsConfig {
                _devApiUrl = config.analytics.devApiUrl,
                _prodApiUrl = config.analytics.prodApiUrl,
            };
        }
    }

    [Serializable]
    public class SerializedTDKConfig
    {
        [Serializable]
        public class SerializedGeneralConfig
        {
            public string cartridgeTag;
            public string cartridgeName;
            public string devApiUrl;
            public string prodApiUrl;
            public string devApiKey;
            public string prodApiKey;
            public string devClientId;
            public string prodClientId;
        }

        [Serializable]
        public class SerializedConnectConfig
        {
            [Serializable]
            public class SerializedSessionOption
            {
                public string chainIdentifier;
                public string backendWallet;
                public List<string> callTargets;
                public double nativeTokenLimitPerTransaction;
            }

            public string factoryAddress;
            public string devDefaultChainIdentifier;
            public string prodDefaultChainIdentifier;
            public int sessionDurationSec;
            public int sessionMinDurationLeftSec;
            public List<SerializedSessionOption> sessionOptions;
        }

        [Serializable]
        public class SerializedAnalyticsConfig
        {
            public string devApiUrl;
            public string prodApiUrl;
        }

        [Serializable]
        public class SerializedHelikaConfig
        {
            public string prodApiKeyWeb;
            public string prodApiKeyIos;
            public string prodApiKeyAndroid;
            public string prodApiKeyDesktop;
            public string devApiKeyWeb;
            public string devApiKeyIos;
            public string devApiKeyAndroid;
            public string devApiKeyDesktop;
        }

        public SerializedGeneralConfig general;
        public SerializedConnectConfig connect;
        public SerializedAnalyticsConfig analytics;
        public SerializedHelikaConfig helika;
    }
}
