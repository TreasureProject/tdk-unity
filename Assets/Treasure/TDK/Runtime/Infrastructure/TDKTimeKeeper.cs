using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;

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
            while (!TDK.Instance.AbstractedEngineApi.HasInternetConnection())
            {
                yield return new WaitForSeconds(RETRY_INTERVAL);
            }

            var endpoint = TDK.Instance.AppConfig.Environment == TDKConfig.Env.PROD ?
                Constants.SERVER_TIME_ENDPOINT_PROD :
                Constants.SERVER_TIME_ENDPOINT_DEV;

            using (UnityWebRequest webRequest = UnityWebRequest.Get(endpoint))
            {
                // Set the API key header
                webRequest.SetRequestHeader("x-api-key", TDK.Instance.AppConfig.ApiKey);

                yield return webRequest.SendWebRequest();

                if (webRequest.result != UnityWebRequest.Result.Success)
                {
                    TDKLogger.LogWarning("[TDKTimeKeeper.GetServerTime] was unable to retrieve UTC time: " + webRequest.error);
                }
                else
                {
                    try
                    {
                        long server = long.Parse(webRequest.downloadHandler.text);
                        TDKLogger.Log("[TDKTimeKeeper.GetServerTime] Got server epoch time: " + server);

                        _epochTimeDiff = server - LocalEpochTime;

                        PlayerPrefs.SetString(Constants.PPREFS_EPOCH_DIFF, _epochTimeDiff.ToString());
                    }
                    catch (Exception e)
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
    }
}
