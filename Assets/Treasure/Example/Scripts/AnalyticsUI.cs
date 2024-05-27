using System.Collections.Generic;
using Treasure;
using UnityEngine;

public class AnalyticsUI : MonoBehaviour
{
    public void OnTrackFtueStartedBtn()
    {
        TDK.Analytics.TrackCustomEvent("ftue_started", new Dictionary<string, object>
        {
            {"step", 0}
        });
    }

    public void OnTrackFtueCompletedBtn()
    {
        TDK.Analytics.TrackCustomEvent("ftue_completed", new Dictionary<string, object>
        {
            {"step", 99}
        });
    }

    public void OnTrackRewardClaimedBtn()
    {
        TDK.Analytics.TrackCustomEvent("reward_claimed", new Dictionary<string, object>
        {
            {"reward_id", "reward_001"}
        });
    }

    public void OnTrackCustomEventBtn()
    {
        TDK.Analytics.TrackCustomEvent("custom_event", new Dictionary<string, object>
        {
            {"custom_event_key", "hello world"}
        });
    }
}
