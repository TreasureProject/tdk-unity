using UnityEngine;
using System.Collections.Generic;

namespace Treasure
{
    public partial class TDK : MonoBehaviour
    {
        /// <summary>
        /// Initialize the Analytics module
        /// </summary>
        private void InitAnalytics()
        {
#if TDK_HELIKA
            TDKServiceLocator.GetService<TDKHelikaService>();
#endif
        }

        private void OnApplicationPause_Analytics(bool isPaused)
        {
            // no-op, but for tracking foreground/background
        }

        public void TrackCustomEvent(string eventName, Dictionary<string, object> eventProps = null)
        {
#if TDK_HELIKA
            TDKServiceLocator.GetService<TDKHelikaService>().TrackEvent(eventName, eventProps);
#endif
        }
    }
}
