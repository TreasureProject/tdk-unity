using UnityEngine;

/**
 * This class is used as the intial measure to extract and isolate engine-specific APIs
 */
namespace Treasure
{
    public class TDKEngineApi : ITDKEngineApi
    {
        public static string ApplicationPersistentDataPath()
        {
            return Application.persistentDataPath;
        }
    }
}