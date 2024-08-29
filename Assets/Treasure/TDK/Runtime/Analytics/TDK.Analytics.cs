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
#if !DISABLE_TREASURE_ANALTYICS
            try
            {
                TDKServiceLocator.GetService<TDKAnalyticsService>().SetTreasureConnectInfo(smartWalletAddress, chainId);
            }
            catch (System.Exception ex)
            {
                TDKLogger.LogException("Error setting connect info for TDKAnalyticsService", ex);
            }
#endif

#if TDK_HELIKA
            try
            {
                TDKServiceLocator.GetService<TDKHelikaService>().SetTreasureConnectInfo(smartWalletAddress, chainId);
            }
            catch (System.Exception ex)
            {
                TDKLogger.LogException("Error setting connect info for TDKHelikaService", ex);
            }
#endif
            TrackCustomEvent(AnalyticsConstants.EVT_TREASURECONNECT_CONNECTED);
        }

        public void TrackCustomEvent(string eventName, Dictionary<string, object> eventProps = null, bool highPriority = false)
        {
            // send events to treasure analytics
#if !DISABLE_TREASURE_ANALTYICS
            try
            {
                TDKServiceLocator.GetService<TDKAnalyticsService>().TrackCustom(eventName, eventProps, highPriority);
            }
            catch (System.Exception ex)
            {
                TDKLogger.LogException("Error tracking event via TDKAnalyticsService", ex);
            }
#endif

            // send events to helika
#if TDK_HELIKA
            try
            {
                TDKServiceLocator.GetService<TDKHelikaService>().TrackEvent(eventName, eventProps);
            }
            catch (System.Exception ex)
            {
                TDKLogger.LogException("Error tracking event via TDKHelikaService", ex);
            }
#endif
        }
    }
}
