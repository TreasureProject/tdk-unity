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

        public enum Env { DEV, PROD }
        public enum LoggerLevelValue { SILENT = 100, ERROR = 40, WARNING = 30, INFO = 20, DEBUG = 10 }

        [SerializeField] private Env _environment = Env.DEV;

        [Header("General")]
        [SerializeField] private string _cartridgeTag;
        [SerializeField] private string _cartridgeName;
        [SerializeField] private Sprite _cartridgeIcon;
        [SerializeField] private string _devApiUrl;
        [SerializeField] private string _prodApiUrl;
        [SerializeField] private string _devApiKey;
        [SerializeField] private string _prodApiKey;
        [SerializeField] private string _devClientId;
        [SerializeField] private string _prodClientId;

        [Header("Connect")] // TODO remove connect prefix from inspector
        [SerializeField] private string _connectFactoryAddress;
        [SerializeField] private ChainId _connectDevDefaultChainId;
        [SerializeField] private ChainId _connectProdDefaultChainId;
        [SerializeField] private int _connectSessionDurationSec;
        [SerializeField] private int _connectSessionMinDurationLeftSec; // TODO use in ValidateActiveSigner
        [SerializeField] private List<SessionOption> _sessionOptions;

        [Header("Analytics")]
        [SerializeField] private string _analyticsDevApiUrl = "https://darkmatter.spellcaster.lol/ingress";
        [SerializeField] private string _analyticsProdApiUrl = "https://darkmatter.treasure.lol/ingress";

        [Header("Modules")]
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

        public string CartridgeTag => _cartridgeTag;
        public string CartridgeName => _cartridgeName;
        public Sprite CartridgeIcon => _cartridgeIcon;

        public string TDKApiUrl => Environment == Env.DEV ? _devApiUrl : _prodApiUrl;
        public string ClientId => Environment == Env.DEV ? _devClientId : _prodClientId;

        public string FactoryAddress => _connectFactoryAddress;

        public ChainId DefaultChainId =>
            Environment == Env.DEV ? _connectDevDefaultChainId : _connectProdDefaultChainId;

        public int SessionLengthSeconds => _connectSessionDurationSec;

        public string AnalyticsApiUrl => Environment == Env.DEV ? _analyticsDevApiUrl : _analyticsProdApiUrl;


        public LoggerLevelValue LoggerLevel
        {
            get { return Environment == Env.DEV ? _devLoggerLevel : _prodLoggerLevel; }
            set { if (Environment == Env.DEV) _devLoggerLevel = value; else _prodLoggerLevel = value; }
        }

        public bool AutoInitialize => _autoInitialize;

        public async Task<string> GetBackendWallet()
        {
            var chainId = await TDK.Connect.GetChainId();
            var option = _sessionOptions.Find(d => d.chainId == chainId);
            return option?.backendWallet.ToLowerInvariant();
        }

        public async Task<List<string>> GetCallTargets()
        {
            var chainId = await TDK.Connect.GetChainId();
            var option = _sessionOptions.Find(d => d.chainId == chainId);
            return option != null ? option.callTargets.ConvertAll(ct => ct.ToLowerInvariant()) : new List<string>();
        }

        public async Task<double> GetNativeTokenLimitPerTransaction()
        {
            var chainId = await TDK.Connect.GetChainId();
            var option = _sessionOptions.Find(d => d.chainId == chainId);
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
            _cartridgeTag = config.general.cartridgeTag;
            _cartridgeName = config.general.cartridgeName;
            _devApiUrl = config.general.devApiUrl;
            _prodApiUrl = config.general.prodApiUrl;
            _devApiKey = config.general.devApiKey;
            _prodApiKey = config.general.prodApiKey;
            _devClientId = config.general.devClientId;
            _prodClientId = config.general.prodClientId;

            // Connect
            _connectFactoryAddress = config.connect.factoryAddress;
            _connectDevDefaultChainId = Constants.NameToChainId.GetValueOrDefault(
                config.connect.devDefaultChainIdentifier ?? "",
                ChainId.Unknown
            );
            _connectProdDefaultChainId = Constants.NameToChainId.GetValueOrDefault(
                config.connect.prodDefaultChainIdentifier ?? "",
                ChainId.Unknown
            );
            _connectSessionDurationSec = config.connect.sessionDurationSec;
            _connectSessionMinDurationLeftSec = config.connect.sessionMinDurationLeftSec;
            if (config.connect.sessionOptions != null)
            {
                _sessionOptions = config.connect.sessionOptions.ConvertAll(option => new SessionOption
                {
                    chainId = Constants.NameToChainId.GetValueOrDefault(option.chainIdentifier, ChainId.Unknown),
                    backendWallet = option.backendWallet,
                    callTargets = option.callTargets,
                    nativeTokenLimitPerTransaction = option.nativeTokenLimitPerTransaction,
                });
            }
            else
            {
                _sessionOptions = new List<SessionOption>();
            }

            // Analytics
            _analyticsDevApiUrl = config.analytics.devApiUrl;
            _analyticsProdApiUrl = config.analytics.prodApiUrl;
        }
    }

    [Serializable]
    public class SerializedTDKConfig
    {
        [Serializable]
        public class GeneralConfig
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
        public class ConnectConfig
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
        public class AnalyticsConfig
        {
            public string devApiUrl;
            public string prodApiUrl;
        }

        [Serializable]
        public class HelikaConfig
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

        public GeneralConfig general;
        public ConnectConfig connect;
        public AnalyticsConfig analytics;
        public HelikaConfig helika;
    }
}
