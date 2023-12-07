using UnityEngine;
using UnityEngine.Serialization;

using System;

namespace Treasure
{
    public class TDKConfig : ScriptableObject
    {
        public static string DEFAULT_CONFIG_LOCATION = "Assets/Treasure/TDK/Resources";

        [SerializeField] private string _treasureApiKey = string.Empty;

        [Serializable] public class ScriptableObjectDictionary : TreasureSerializableDictionary<string, ScriptableObject> {}
        [SerializeField] ScriptableObjectDictionary moduleConfigurations = null;

        public T GetModuleConfig<T>()
        {
            if(moduleConfigurations.ContainsKey(typeof(T).Name))
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
            _treasureApiKey = config.apiKey;
        }
    }

    [Serializable]
	public class SerializedTDKConfig
	{
		[SerializeField] public string apiKey;
	}
}
