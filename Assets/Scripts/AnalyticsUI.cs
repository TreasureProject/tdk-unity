using System.Collections;
using System.Collections.Generic;
using Treasure;
using UnityEngine;

public class AnalyticsUI : MonoBehaviour
{
    public void OnTrackCustomEventBtn()
    {
        TDK.Instance.TrackCustomEvent("custom_event", new Dictionary<string, object>
        {
            {"custom_event_key", "hello world"}
        });
    }
}
