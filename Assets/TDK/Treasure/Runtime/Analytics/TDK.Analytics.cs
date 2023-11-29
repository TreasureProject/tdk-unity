using UnityEngine;

namespace Treasure
{
    public partial class TDK : MonoBehaviour
    {   
        /// <summary>
        /// Initialize the Analytics module
        /// </summary>
        private void InitAnalytics()
        {
             // #if TDK_HELIKA // TODO re-enable and add scripting define
            TDKServiceLocator.GetService<TDKHelikaService>();
            // #endif
        }

        private void OnApplicationPause_Analytics(bool isPaused)
        {
            // no-op, but for tracking foreground/background
        }
    }
}
