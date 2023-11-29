using UnityEngine;

namespace Treasure
{
    public class TDKBaseService : MonoBehaviour
    {
        public virtual void Awake()
        {
            DontDestroyOnLoad(this);
        }
    }
}
