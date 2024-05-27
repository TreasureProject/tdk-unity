using UnityEngine;
using System;

namespace Treasure
{
    public partial class TDK : MonoBehaviour
    {
        /// <summary>
        /// Singleton to access all Treasure TDK functionality through
        /// </summary>
        private static TDK _instance = null;

        public static bool Initialized { get; private set; }

        public TDKConfig AppConfig { get; private set; }

        public string PersistentDataPath { get; set; }

        void OnApplicationPause(bool isPaused)
        {
            TDK.Analytics.OnApplicationPause_Analytics(isPaused);
        }

        public static TDK Instance
        {
            get
            {
                // setup singleton
                if (_instance == null)
                {
                    _instance = GameObject.FindObjectOfType(typeof(TDK)) as TDK;

                    if (_instance == null)
                    {
                        // create a new instance
                        _instance = new GameObject("TDK", new Type[] {
                            typeof(TDK),
                            typeof(TDKTimeKeeper)
                        }).GetComponent<TDK>();

                        DontDestroyOnLoad(_instance.gameObject);
                    }

                }

                TDKMainThreadDispatcher.Instance.Enqueue(() =>
                {
                    // no-op; don't add TDKLogger calls here
                });

                return _instance;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoInitialize()
        {
            Instance.AppConfig = TDKConfig.LoadFromResources();

            // initialize subsystems
            Instance.InitCommon();
            Instance.InitAnalytics();
            Instance.InitAPI();
            Instance.InitIdentity();
            Instance.InitConnect();
            Instance.InitBridgeworld();

            // track app start event
#if TDK_ANALYTICS
            TDKServiceLocator.GetService<TDKAnalyticsService>().TrackCustom(AnalyticsConstants.EVT_APP_START);
#endif

            // set as initialized
            Initialized = true;
        }
    }
}
