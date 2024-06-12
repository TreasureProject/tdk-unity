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
        public void InitAnalytics() // TODO is it ok to be public?
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

        internal void SetTreasureConnectInfo(string smartWalletAddress, int chainId)
        {
#if TREASURE_ANALYTICS
            TDKServiceLocator.GetService<TDKAnalyticsService>().SetTreasureConnectInfo(smartWalletAddress, chainId);
#endif

#if TDK_HELIKA
            TDKServiceLocator.GetService<TDKHelikaService>().SetPlayerId(smartWalletAddress);
#endif
            TrackCustomEvent(AnalyticsConstants.EVT_TREASURECONNECT_CONNECTED, new Dictionary<string, object>()
            {
                { AnalyticsConstants.PROP_CHAIN_ID, chainId }
            });
        }

        public void TrackCustomEvent(string eventName, Dictionary<string, object> eventProps = null, bool highPriority = false)
        {
            // send events to treasure analytics
#if TREASURE_ANALYTICS
            TDKServiceLocator.GetService<TDKAnalyticsService>().TrackCustom(eventName, eventProps, highPriority);
#endif

            // send events to helika
#if TDK_HELIKA
            Debug.Log("helika " + eventName);
            TDKServiceLocator.GetService<TDKHelikaService>().TrackEvent(eventName, eventProps);
#endif
        }
    }
}
