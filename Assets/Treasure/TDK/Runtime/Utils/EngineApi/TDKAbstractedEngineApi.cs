using UnityEngine;

/**
 * This class is used as the intial measure to extract and isolate engine-specific APIs
 */
namespace Treasure
{
    public class TDKAbstractedEngineApi : IAbstractedEngineApi
    {
        private string _persistentDataPath;

        public TDKAbstractedEngineApi()
        {
            _persistentDataPath = Application.persistentDataPath;
        }

        public string ApplicationPersistentDataPath()
        {
            return _persistentDataPath;
        }

        public T GetPersistedValue<T>(string key)
        {
            if (PlayerPrefs.HasKey(key))
            {
                string value = PlayerPrefs.GetString(key);
                return JsonUtility.FromJson<T>(value);
            }
            return default(T);
        }

        public void SetPersistedValue<T>(string key, T value)
        {
            string json = JsonUtility.ToJson(value);
            PlayerPrefs.SetString(key, json);
            PlayerPrefs.Save();
        }

        public void DeletePersistedValue(string key)
        {
            PlayerPrefs.DeleteKey(key);
            PlayerPrefs.Save();
        }
    }
}