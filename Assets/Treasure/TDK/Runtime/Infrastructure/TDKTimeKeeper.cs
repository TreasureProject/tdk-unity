using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using Newtonsoft.Json;

namespace Treasure
{
    public class TDKTimeKeeper : MonoBehaviour
    {
        private const float RETRY_INTERVAL = 5f;

        private static double _epochTimeDiff = double.NaN;

        private void Start()
        {
            StartCoroutine(GetServerTime());
        }

        private IEnumerator GetServerTime()
        {
            while (Application.internetReachability == NetworkReachability.NotReachable)
            {
                yield return new WaitForSeconds(RETRY_INTERVAL);
            }

            using (UnityWebRequest webRequest = UnityWebRequest.Get(Constants.SERVER_TIME_ENDPOINT))
            {
                yield return webRequest.SendWebRequest();

                if (webRequest.result != UnityWebRequest.Result.Success)
                {
                    TDKLogger.LogWarning("[TDKTimeKeeper.GetServerTime] was unable to retrieve UTC time: " + webRequest.error);
                }
                else
                {
                    string jsonResponse = webRequest.downloadHandler.text;
                    try
                    {
                        // deserialize the JSON string into your data model
                        ServerTimeResponse responseData = JsonConvert.DeserializeObject<ServerTimeResponse>(jsonResponse);

                        // convert UTC time to epoch time
                        DateTime utcDateTime = DateTime.Parse(responseData.utc);
                        long server = (long)(utcDateTime - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;

                        var local = LocalEpochTime;
                        
                        _epochTimeDiff = server - local;
                        
                        PlayerPrefs.SetString(Constants.PPREFS_EPOCH_DIFF, _epochTimeDiff.ToString()); 
                        TDKLogger.Log("[TDKTimeKeeper.GetServerTime] Got server epoch time: " + server);
                    }
                    catch(Exception e)
                    {
                        TDKLogger.LogWarning("[TDKTimeKeeper.GetServerTime] was unable to parse UTC time" + e.ToString());
                    }
                }
            }
        }

        public static void ResetSync()
        {
            _epochTimeDiff = double.NaN;

            var timekeeper = TDK.Instance.GetComponent(typeof(TDKTimeKeeper)) as TDKTimeKeeper;

            if (timekeeper != null)
            {
                timekeeper.StartCoroutine(timekeeper.GetServerTime());
            }
        }

        public static double LocalToServerEpochTimeDiff
        {
            get
            {
                if (double.IsNaN(_epochTimeDiff) && PlayerPrefs.HasKey(Constants.PPREFS_EPOCH_DIFF))
                {
                    var diff = PlayerPrefs.GetString(Constants.PPREFS_EPOCH_DIFF);
                    _epochTimeDiff = Convert.ToDouble(diff);
                }

                return _epochTimeDiff;
            }
        }

        public static bool ServerTimeReady
        {
            get { return !double.IsNaN(LocalToServerEpochTimeDiff); }
        }

        public static double LocalEpochTime
        {
            get
            {
                var t = DateTime.UtcNow - new DateTime(1970, 1, 1);
                return t.TotalMilliseconds;
            }
        }

        public static long LocalEpochTimeInt64
        {
            get { return Convert.ToInt64(LocalEpochTime); }
        }

        public static double ServerEpochTime
        {
            get
            {
                if (!ServerTimeReady)
                {
                    return double.NaN;
                }

                return LocalEpochTime + LocalToServerEpochTimeDiff;
            }
        }

        public static long ServerEpochTimeInt64
        {
            get { return Convert.ToInt64(ServerEpochTime); }
        }

        public class ServerTimeResponse
        {
            public string utc { get; set; }
            public string local { get; set; }
        }
    }
}
