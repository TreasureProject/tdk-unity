using UnityEngine;
using System;

namespace Treasure
{
    public class TDKConfig : ScriptableObject
    {
        public static string DEFAULT_CONFIG_LOCATION = "Assets/Treasure/TDK/Resources";

        public enum Env { DEV, PROD }

        [SerializeField] private Env _environment = Env.DEV;
        [SerializeField] private string _cartridgeTag = string.Empty;
        [SerializeField] private string _devTdkApiUrl = "https://tdk-api-dev.treasure.lol";
        [SerializeField] private string _prodTdkApiUrl = "https://tdk-api.treasure.lol";
        [SerializeField] private string _devAnalyticsApiUrl = "https://darkmatter-dev.treasure.lol/ingress";
        [SerializeField] private string _prodAnalyticsApiUrl = "https://darkmatter.treasure.lol/ingress";
        [SerializeField] private int _sessionLengthDays = 3;

        [Serializable] public class ScriptableObjectDictionary : TreasureSerializableDictionary<string, ScriptableObject> { }
        [SerializeField] ScriptableObjectDictionary moduleConfigurations = null;

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
