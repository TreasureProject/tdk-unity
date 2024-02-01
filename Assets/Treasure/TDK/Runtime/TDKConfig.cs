using UnityEngine;
using System;

namespace Treasure
{
    public class TDKConfig : ScriptableObject
    {
        public static string DEFAULT_CONFIG_LOCATION = "Assets/Treasure/TDK/Resources";

        [SerializeField] private string _environment = string.Empty; // prod/dev - TODO change to enum?
        [SerializeField] private string _gameId = string.Empty;
        [SerializeField] private string _tdkApiUrl = "https://tdk-api-dev.treasure.lol";


        [Serializable] public class ScriptableObjectDictionary : TreasureSerializableDictionary<string, ScriptableObject> { }
        [SerializeField] ScriptableObjectDictionary moduleConfigurations = null;

        public string Environment
        {
            get { return _environment; }
        }

        public string GameId
        {
            get { return _gameId; }
        }

        public string TDKApiUrl
        {
            get { return _tdkApiUrl; }
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
            _tdkApiUrl = config.tdkApiUrl;
        }
    }

    [Serializable]
    public class SerializedTDKConfig
    {
        [SerializeField] public string gameId;
        [SerializeField] public string tdkApiUrl;
    }
}
