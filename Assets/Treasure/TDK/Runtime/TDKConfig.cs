using UnityEngine;

namespace Treasure
{
    public class TDKConfig : ScriptableObject
    {
        public static TDKConfig LoadFromResources()
        {
            return Resources.Load<TDKConfig>("TDKConfig");
        }
    }
}
