using UnityEngine;
using System.Collections.Generic;

namespace Treasure
{
    public partial class TDK : MonoBehaviour
    {
        public static Analytics Analytics { get; private set; }

        /// <summary>
        /// Initialize the Analytics module
        /// </summary>
        private void InitAnalytics()
        {
            Analytics = new Analytics();

#if TDK_HELIKA
            TDKServiceLocator.GetService<TDKHelikaService>();
#endif
        }
    }

    public class Analytics
    {
        public Analytics() { }

        public void OnApplicationPause_Analytics(bool isPaused)
        {
            // no-op, but for tracking foreground/background
        }

        public void TrackCustomEvent(string eventName, Dictionary<string, object> eventProps = null)
        {
            TDKServiceLocator.GetService<TDKAnalyticsService>().TrackCustom(eventName, eventProps);
#if TDK_HELIKA
            TDKServiceLocator.GetService<TDKHelikaService>().TrackEvent(eventName, eventProps);
#endif
        }
    }
}
