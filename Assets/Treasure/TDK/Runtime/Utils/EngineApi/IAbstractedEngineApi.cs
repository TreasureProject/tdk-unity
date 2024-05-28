using UnityEngine;

namespace Treasure
{
    public interface IAbstractedEngineApi
    {
        public void Init();
        public string ApplicationPersistentDataPath();
    }
}