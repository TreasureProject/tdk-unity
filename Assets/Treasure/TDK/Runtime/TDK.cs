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
        public static TDKConfig AppConfig { get; private set; }

        private IAbstractedEngineApi _abstractedEngineApi;
        private LocalSettings _localsettings;

        private static bool _appIsQuitting = false;

        void OnApplicationQuit() {
            _appIsQuitting = true;
        }

        void OnApplicationPause(bool isPaused)
        {
            Analytics?.OnApplicationPause_Analytics(isPaused);
        }

        public static TDK Instance
        {
            get
            {
                if (_appIsQuitting) {
                    return _instance; // Prevent recreating instance if app is quitting
                }
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

                TDKMainThreadDispatcher.StartProcessing();

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
                    abstractedEngineApi: new TDKAbstractedEngineApi(),
                    localSettings: new LocalSettings(Application.persistentDataPath)
                );
            }
        }

        public static void Initialize(
            TDKConfig tdkConfig,
            IAbstractedEngineApi abstractedEngineApi,
            LocalSettings localSettings
        )
        {
            if (Initialized) {
                return;
            }
            Instance.gameObject.AddComponent<TDKTimeKeeper>();
            Instance.InitializeProperties(tdkConfig, abstractedEngineApi, localSettings);
            Instance.InitializeSubsystems();

            // track app start event
#if !DISABLE_TREASURE_ANALTYICS
            TDKServiceLocator.GetService<TDKAnalyticsService>().TrackCustom(AnalyticsConstants.EVT_APP_START);
#endif
            // set as initialized
            Initialized = true;
        }

        private void InitializeProperties(
            TDKConfig tdkConfig,
            IAbstractedEngineApi abstractedEngineApi,
            LocalSettings localSettings
        ) {
            AppConfig = tdkConfig;

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
