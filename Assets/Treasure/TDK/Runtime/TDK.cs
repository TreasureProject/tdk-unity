using UnityEngine;
using System;
using System.Collections;

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
        public AppSettingsData AppSettingsData { get; private set; }

        void OnApplicationPause(bool isPaused)
        {
            Analytics?.OnApplicationPause_Analytics(isPaused);
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
                            typeof(TDK)
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
            var tdkConfig = TDKConfig.LoadFromResources();
            if (tdkConfig.AutoInitialize) {
                Initialize(
                    tdkConfig: tdkConfig,
                    appSettingsData: AppSettingsData.LoadFromResources(),
                    abstractedEngineApi: new TDKAbstractedEngineApi(),
                    localSettings: new LocalSettings(Application.persistentDataPath)
                );
            }
        }

        public static void Initialize(
            TDKConfig tdkConfig,
            AppSettingsData appSettingsData,
            IAbstractedEngineApi abstractedEngineApi,
            LocalSettings localSettings
        )
        {
            if (Initialized) {
                return;
            }
            Instance.gameObject.AddComponent<TDKTimeKeeper>();
            Instance.InitializeProperties(tdkConfig, appSettingsData, abstractedEngineApi, localSettings);
            Instance.InitializeSubsystems();

            // track app start event
#if TREASURE_ANALYTICS
            TDKServiceLocator.GetService<TDKAnalyticsService>().TrackCustom(AnalyticsConstants.EVT_APP_START);
#endif
            // set as initialized
            Initialized = true;
        }

        private void InitializeProperties(
            TDKConfig tdkConfig,
            AppSettingsData appSettingsData,
            IAbstractedEngineApi abstractedEngineApi,
            LocalSettings localSettings
        ) {
            Instance.AppConfig = tdkConfig;
            Instance.AppSettingsData = appSettingsData;

            Instance._abstractedEngineApi = abstractedEngineApi;
            Instance._localsettings = localSettings;
        }

        private void InitializeSubsystems() {
            InitCommon();
            InitAnalytics();
            InitAPI();
            InitIdentity();
            InitConnect();
            InitBridgeworld();
            InitMagicswap();
        }
    }
}
