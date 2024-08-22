using UnityEngine;

namespace Treasure
{
    // TODO condider if this class is needed.
    // why do we need empty game objects for services? it could be independent from unity
    public class TDKBaseService : MonoBehaviour
    {
        protected bool appIsQuitting = false;

        public virtual void Awake()
        {
            DontDestroyOnLoad(this);
        }
    }
}
