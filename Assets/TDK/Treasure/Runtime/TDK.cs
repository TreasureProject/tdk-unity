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

        void OnApplicationPause(bool isPaused)
		{
            OnApplicationPause_Analytics(isPaused);
		}

        public static TDK Instance
		{
			get
			{
                // setup singleton
				if (_instance == null)
				{
					_instance = GameObject.FindObjectOfType(typeof(TDK)) as TDK;

                    if (_instance == null )
					{
						// create a new instance
						_instance = new GameObject("TDK", new Type[] {
							typeof(TDK)
						}).GetComponent<TDK>();

						DontDestroyOnLoad(_instance.gameObject);
                    }
                }

                return _instance;
            }
        }

        // TODO wrap AutoInitialize in scripting define to enable manual / a la carte inits
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoInitialize()
        {
            Init();

            // initialize subsystems
            Instance.InitAnalytics();
            Instance.InitIdentity();
        }

        public static void Init()
        {
            Instance.AppConfig = TDKConfig.LoadFromResources();
            Initialized = true;
        }
    }
}
