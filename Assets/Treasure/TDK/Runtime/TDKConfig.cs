using UnityEngine;
using System;

namespace Treasure
{
    public class TDKConfig : ScriptableObject
    {
        public static string DEFAULT_CONFIG_LOCATION = "Assets/Treasure/TDK/Resources";

        public enum Env { DEV, PROD }
        public enum LoggerLevelValue { SILENT = 100, ERROR = 40, WARNING = 30, INFO = 20, DEBUG = 10 }

        [SerializeField] private Env _environment = Env.DEV;
        [SerializeField] private string _cartridgeTag = string.Empty;
        [SerializeField] private string _devTdkApiUrl = "https://tdk-api.spellcaster.lol";
        [SerializeField] private string _prodTdkApiUrl = "https://tdk-api.treasure.lol";
        [SerializeField] private string _devAnalyticsApiUrl = "https://darkmatter.spellcaster.lol/ingress";
        [SerializeField] private string _prodAnalyticsApiUrl = "https://darkmatter.treasure.lol/ingress";
        [SerializeField] private int _sessionLengthDays = 3;

        [Serializable] public class ScriptableObjectDictionary : TreasureSerializableDictionary<string, ScriptableObject> { }
        [SerializeField] ScriptableObjectDictionary moduleConfigurations = null;

        [SerializeField] private LoggerLevelValue _devLoggerLevel = LoggerLevelValue.INFO;
        [SerializeField] private LoggerLevelValue _prodLoggerLevel = LoggerLevelValue.INFO;
        [SerializeField] private bool _autoInitialize = true;

        public Env Environment
        {
            get { return _environment; }
            set { _environment = value; }
        }

        public string CartridgeTag
        {
            get { return _cartridgeTag; }
        }

        public string TDKApiUrl
        {
            get { return Environment == Env.DEV ? _devTdkApiUrl : _prodTdkApiUrl; }
        }

        public string AnalyticsApiUrl
        {
            get { return Environment == Env.DEV ? _devAnalyticsApiUrl : _prodAnalyticsApiUrl; }
        }

        public int SessionLengthDays
        {
            get { return _sessionLengthDays; }
        }

        public LoggerLevelValue LoggerLevel
        {
            get { return Environment == Env.DEV ? _devLoggerLevel : _prodLoggerLevel; }
            set { if (Environment == Env.DEV) _devLoggerLevel = value; else _prodLoggerLevel = value; }
        }

        public bool AutoInitialize
        {
            get { return _autoInitialize; }
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
            if (moduleConfigurations == null) {
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
            _cartridgeTag = config.cartridgeTag;
            _devTdkApiUrl = config.devTdkApiUrl;
            _prodTdkApiUrl = config.prodTdkApiUrl;
            _devAnalyticsApiUrl = config.devAnalyticsApiUrl;
            _prodAnalyticsApiUrl = config.prodAnalyticsApiUrl;
            _sessionLengthDays = config.sessionLengthDays;
        }
    }

    [Serializable]
    public class SerializedTDKConfig
    {
        [SerializeField] public string cartridgeTag;
        [SerializeField] public string devTdkApiUrl;
        [SerializeField] public string prodTdkApiUrl;
        [SerializeField] public string devAnalyticsApiUrl;
        [SerializeField] public string prodAnalyticsApiUrl;
        [SerializeField] public int sessionLengthDays;
    }
}
