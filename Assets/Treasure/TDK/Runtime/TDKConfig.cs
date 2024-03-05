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
        [SerializeField] private string _prodTdkApiUrl = string.Empty;
        [SerializeField] private string _devTdkApiUrl = string.Empty;
        [SerializeField] private string _analyticsApiUrl = string.Empty;
        [SerializeField] private float _sessionLengthDays = 0;

        [Serializable] public class ScriptableObjectDictionary : TreasureSerializableDictionary<string, ScriptableObject> { }
        [SerializeField] ScriptableObjectDictionary moduleConfigurations = null;

        public Env Environment
        {
            get { return _environment; }
        }

        public string CartridgeTag
        {
            get { return _cartridgeTag; }
        }

        public string TDKApiUrl
        {
            get
            {
                if (TDK.Instance.AppConfig.Environment == TDKConfig.Env.PROD)
                {
                    return _prodTdkApiUrl;
                }
                else
                {
                    return _devTdkApiUrl;
                }
            }
        }

        public string AnalyticsApiUrl
        {
            get { return _analyticsApiUrl; }
        }

        public float SessionLengthDays
        {
            get { return _sessionLengthDays; }
        }

        public T GetModuleConfig<T>()
        {
            if (moduleConfigurations.ContainsKey(typeof(T).Name))
            {
                return (T)Convert.ChangeType(moduleConfigurations[typeof(T).Name], typeof(T));
            }
            return default(T);
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
            _prodTdkApiUrl = config.prodTdkApiUrl;
            _devTdkApiUrl = config.devTdkApiUrl;
            _analyticsApiUrl = config.analyticsApiUrl;
            _sessionLengthDays = config.sessionLengthDays;
        }
    }

    [Serializable]
    public class SerializedTDKConfig
    {
        [SerializeField] public string cartridgeTag;
        [SerializeField] public string prodTdkApiUrl;
        [SerializeField] public string devTdkApiUrl;
        [SerializeField] public string analyticsApiUrl;
        [SerializeField] public float sessionLengthDays;
    }
}
