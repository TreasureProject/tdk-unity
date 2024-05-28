using UnityEngine;

/**
 * This class is used as the intial measure to extract and isolate engine-specific APIs
 */
namespace Treasure
{
    public class TDKAbstractedEngineApi : IAbstractedEngineApi
    {
        private string _persistentDataPath;

        public void Init()
        {
            _persistentDataPath = Application.persistentDataPath;
        }

        public string ApplicationPersistentDataPath()
        {
            return _persistentDataPath;
        }
    }
}