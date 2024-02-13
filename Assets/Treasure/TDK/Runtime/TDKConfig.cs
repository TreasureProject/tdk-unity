using UnityEngine;
using System;

namespace Treasure
{
    public class TDKConfig : ScriptableObject
    {
        public static string DEFAULT_CONFIG_LOCATION = "Assets/Treasure/TDK/Resources";

        public enum Env { DEV, PROD }

        [SerializeField] private Env _environment = Env.DEV;
        [SerializeField] private string _gameId = string.Empty;
        [SerializeField] private string _prodTdkApiUrl = string.Empty;
        [SerializeField] private string _devTdkApiUrl = string.Empty;
        [SerializeField] private float _sessionLengthDays = 0;

        [Serializable] public class ScriptableObjectDictionary : TreasureSerializableDictionary<string, ScriptableObject> { }
        [SerializeField] ScriptableObjectDictionary moduleConfigurations = null;

        public Env Environment
        {
            get { return _environment; }
        }

        public string GameId
        {
            get { return _gameId; }
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
            _gameId = config.gameId;
            _prodTdkApiUrl = config.prodTdkApiUrl;
            _devTdkApiUrl = config.devTdkApiUrl;
            _sessionLengthDays = config.sessionLengthDays;
        }
    }

    [Serializable]
    public class SerializedTDKConfig
    {
        [SerializeField] public string gameId;
        [SerializeField] public string prodTdkApiUrl;
        [SerializeField] public string devTdkApiUrl;
        [SerializeField] public float sessionLengthDays;
    }
}
