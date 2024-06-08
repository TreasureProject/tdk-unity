#if UNITY_WEBGL
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System;

namespace Treasure
{
    public partial class TDKAnalyticsService : TDKBaseService
    {
        private void StartNewSession()
        {
            _sessionId = Guid.NewGuid().ToString("N");
        }

        /// <summary>
        /// All other tracking methods should route through this function
        /// </summary>
        /// <param name="eventName">The name of the event</param>
        /// <param name="eventProps">Event properties</param>
        /// <devnote>Avoid perf hit of compiler async state machine creation for WebGL; hence the two methods.</devnote>
        public void TrackCustom(string eventName, Dictionary<string, object> eventProps = null, bool highPriority = false)
        {
            // serialize the event to JSON
            string jsonEvtStr = JsonConvert.SerializeObject(BuildBaseEvent(eventName, eventProps));

            StartCoroutine(SendEvent(jsonEvtStr));
        }

        /// <summary>
        /// Flush all pending events to the analytics backend
        /// </summary>
        /// <devnote>Avoid perf hit of compiler async state machine creation for WebGL; hence the two methods.</devnote>
        public void FlushCache() {}
    }
}
#endif
