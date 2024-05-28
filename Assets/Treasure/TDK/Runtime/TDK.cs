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

        private IAbstractedEngineApi _abstractedEngineApi;
        private LocalSettings _localsettings;

        public TDKConfig AppConfig { get; private set; }

        void OnApplicationPause(bool isPaused)
        {
            Analytics.OnApplicationPause_Analytics(isPaused);
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

        public IAbstractedEngineApi AbstractedEngineApi
        {
            get { return Instance._abstractedEngineApi; }
        }

        public LocalSettings LocalSettings
        {
            get { return Instance._localsettings; }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoInitialize()
        {
            Instance.AppConfig = TDKConfig.LoadFromResources();

            Instance._abstractedEngineApi = new TDKAbstractedEngineApi();
            Instance._localsettings = new LocalSettings(Application.persistentDataPath);

            // initialize subsystems
            Instance.InitCommon();
            Instance.InitAnalytics();
            Instance.InitAPI();
            Instance.InitIdentity();
            Instance.InitConnect();
            Instance.InitBridgeworld();

            // track app start event
#if TREASURE_ANALYTICS
            TDKServiceLocator.GetService<TDKAnalyticsService>().TrackCustom(AnalyticsConstants.EVT_APP_START);
#endif

            // set as initialized
            Initialized = true;
        }
    }
}
