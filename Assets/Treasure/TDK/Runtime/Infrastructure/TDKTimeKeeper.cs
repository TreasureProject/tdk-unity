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

        private static long _epochTimeDiff = 0;

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

            var endpoint = TDK.Instance.AppConfig.Environment == TDKConfig.Env.PROD ? Constants.SERVER_TIME_ENDPOINT_PROD : Constants.SERVER_TIME_ENDPOINT_DEV;

            using (UnityWebRequest webRequest = UnityWebRequest.Get(endpoint))
            {
                yield return webRequest.SendWebRequest();

                if (webRequest.result != UnityWebRequest.Result.Success)
                {
                    TDKLogger.LogWarning("[TDKTimeKeeper.GetServerTime] was unable to retrieve UTC time: " + webRequest.error);
                }
                else
                {
                    long server = long.Parse(webRequest.downloadHandler.text);
                    // TDKLogger.Log();
                    try
                    {
                        // deserialize the JSON string into your data model
                        // ServerTimeResponse responseData = JsonConvert.DeserializeObject<ServerTimeResponse>(jsonResponse);
                        // TDKLogger.Log("[TDKTimeKeeper.GetServerTime] ServerTimeResponse: " + serverTimeResponseStr);

                        // convert UTC time to epoch time
                        // DateTime utcDateTime = DateTime.Parse(serverTimeResponseStr);
                        // DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(long.Parse(serverTimeResponseStr));
                        // DateTime dateTime = dateTimeOffset.UtcDateTime;
                        // long server = (long)(utcDateTime - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
                        
                        _epochTimeDiff = server - LocalEpochTime;
                        
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
            _epochTimeDiff = 0;

            var timekeeper = TDK.Instance.GetComponent(typeof(TDKTimeKeeper)) as TDKTimeKeeper;

            if (timekeeper != null)
            {
                timekeeper.StartCoroutine(timekeeper.GetServerTime());
            }
        }

        public static long LocalEpochTime
        {
            get { return DateTimeOffset.Now.ToUnixTimeMilliseconds(); }
        }

        public static double ServerEpochTime
        {
            get { return LocalEpochTime + _epochTimeDiff; }
        }

        // public class ServerTimeResponse
        // {
        //     public string utc { get; set; }
        //     public string local { get; set; }
        // }
    }
}
