using UnityEngine;

namespace Treasure
{
    public interface IAbstractedEngineApi
    {
        public string ApplicationPersistentDataPath();

        public T GetPersistedValue<T>(string key);
        public void SetPersistedValue<T>(string key, T value);
        public void DeletePersistedValue(string key);

        public bool HasInternetConnection();
    }
}