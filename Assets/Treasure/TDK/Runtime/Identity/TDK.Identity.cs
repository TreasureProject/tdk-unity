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
            #if TDK_THIRDWEB
            TDKServiceLocator.GetService<TDKThirdwebService>();
            #endif
        }

        public string AuthToken
        {
            get {
                return TDKServiceLocator.GetService<TDKThirdwebService>().AuthToken;
            }
        }
    }
}
