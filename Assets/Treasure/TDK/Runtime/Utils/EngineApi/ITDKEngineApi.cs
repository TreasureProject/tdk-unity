using UnityEngine;

namespace Treasure
{
    public interface ITDKEngineApi
    {
        public string ApplicationPersistentDataPath()
        {
            return Application.persistentDataPath;
        }
    }
}