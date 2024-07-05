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

        public virtual string ApplicationPersistentDataPath()
        {
            return _persistentDataPath;
        }

        public virtual T GetPersistedValue<T>(string key)
        {
            if (PlayerPrefs.HasKey(key))
            {
                string value = PlayerPrefs.GetString(key);
                return JsonUtility.FromJson<T>(value);
            }
            return default(T);
        }

        public virtual void SetPersistedValue<T>(string key, T value)
        {
            string json = JsonUtility.ToJson(value);
            PlayerPrefs.SetString(key, json);
            PlayerPrefs.Save();
        }

        public virtual void DeletePersistedValue(string key)
        {
            PlayerPrefs.DeleteKey(key);
            PlayerPrefs.Save();
        }

        public bool HasInternetConnection()
        {
            return Application.internetReachability != NetworkReachability.NotReachable;
        }
    }
}