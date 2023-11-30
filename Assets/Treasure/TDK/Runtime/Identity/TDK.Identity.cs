using UnityEngine;

namespace Treasure
{
    public partial class TDK : MonoBehaviour
    {   
        /// <summary>
        /// Initialize the Identity module
        /// </summary>
        private void InitIdentity()
        {
            // #if TDK_THIRDWEB // TODO re-enable and add scripting define
            TDKServiceLocator.GetService<TDKThirdwebService>();
            // #endif
        }
    }
}
