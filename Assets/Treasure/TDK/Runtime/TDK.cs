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
        public static bool skipAutoInitialize = false;

        private IAbstractedEngineApi _abstractedEngineApi;
        private LocalSettings _localsettings;

        public TDKConfig AppConfig { get; private set; }

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
            Instance.StartCoroutine(AutoInitializeCoroutine());
        }

        private static IEnumerator AutoInitializeCoroutine()
        {
            // TODO check if waiting is okay:
            // we wait to allow other code to set skipAutoInitialize (eg: tests) before requests are triggered
            yield return new WaitForSeconds(1f);
            if (skipAutoInitialize) {
                Initialized = true;
                yield break;
            }

            Instance.gameObject.AddComponent<TDKTimeKeeper>();
            Instance.InitProperties(
                tdkConfig: TDKConfig.LoadFromResources(),
                abstractedEngineApi: new TDKAbstractedEngineApi(),
                localSettings: new LocalSettings(Application.persistentDataPath)
            );
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

        public void InitProperties(TDKConfig tdkConfig, IAbstractedEngineApi abstractedEngineApi, LocalSettings localSettings) {
            Instance.AppConfig = tdkConfig;

            Instance._abstractedEngineApi = abstractedEngineApi;
            Instance._localsettings = localSettings;
        }
    }
}
