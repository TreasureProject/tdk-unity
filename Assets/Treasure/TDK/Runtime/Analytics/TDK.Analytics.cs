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

        internal void SetTreasureConnectInfo(string smartWalletAddress, int chainId)
        {
#if TDK_ANALYTICS
            TDKServiceLocator.GetService<TDKAnalyticsService>().SetTreasureConnectInfo(smartWalletAddress, chainId);
#endif

#if TDK_HELIKA
            TDKServiceLocator.GetService<TDKHelikaService>().SetPlayerId(smartWalletAddress);
#endif
            TrackCustomEvent(AnalyticsConstants.EVT_CONNECT_CONNECTED, new Dictionary<string, object>()
            {
                { AnalyticsConstants.PROP_CHAIN_ID, chainId }
            });
        }

        public void TrackCustomEvent(string eventName, Dictionary<string, object> eventProps = null)
        {
            // send events to treasure analytics
#if TDK_ANALYTICS
            TDKServiceLocator.GetService<TDKAnalyticsService>().TrackCustom(eventName, eventProps);
#endif

            // send events to helika
#if TDK_HELIKA
            TDKServiceLocator.GetService<TDKHelikaService>().TrackEvent(eventName, eventProps);
#endif
        }
    }
}
