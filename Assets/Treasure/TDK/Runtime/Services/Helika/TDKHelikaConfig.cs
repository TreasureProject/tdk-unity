using UnityEngine;
using System;

namespace Treasure
{
    public class TDKHelikaConfig : ScriptableObject
    {
        [SerializeField] public string _prodApiKeyWeb;
        [SerializeField] public string _prodApiKeyIos;
        [SerializeField] public string _prodApiKeyAndroid;
        [SerializeField] public string _prodApiKeyDesktop;
        [SerializeField] public string _devApiKeyWeb;
        [SerializeField] public string _devApiKeyIos;
        [SerializeField] public string _devApiKeyAndroid;
        [SerializeField] public string _devApiKeyDesktop;

        public string ApiKey
        {
            get {
                if(TDK.Instance.AppConfig.Environment == TDKConfig.Env.PROD) {
                    #if UNITY_IOS
                    return _prodApiKeyIos;
                    #elif UNITY_ANDROID
                    return _prodApiKeyAndroid;
                    #elif UNITY_WEBGL
                    return _prodApiKeyWeb;
                    #elif UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX
                    return _prodApiKeyDesktop;
                    #else
                    throw new Exception("Unsupported platform");
                    #endif
                }
                else {
                    #if UNITY_IOS
                    return _devApiKeyIos;
                    #elif UNITY_ANDROID
                    return _devApiKeyAndroid;
                    #elif UNITY_WEBGL
                    return _devApiKeyWeb;
                    #elif UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX
                    return _devApiKeyDesktop;
                    #else
                    throw new Exception("Unsupported platform");
                    #endif
                }
            }
        }

        public void SetConfig(SerializedHelikaConfig config)
        {
            _prodApiKeyWeb = config.prodApiKeyWeb;
            _prodApiKeyIos = config.prodApiKeyIos;
            _prodApiKeyAndroid = config.prodApiKeyAndroid;
            _prodApiKeyDesktop = config.prodApiKeyDesktop;
            _devApiKeyWeb = config.devApiKeyWeb;
            _devApiKeyIos = config.devApiKeyIos;
            _devApiKeyAndroid = config.devApiKeyAndroid;
            _devApiKeyDesktop = config.devApiKeyDesktop;
        }
    }

    [Serializable]
    public class SerializedHelikaConfig
    {
        [SerializeField] public string prodApiKeyWeb;
        [SerializeField] public string prodApiKeyIos;
        [SerializeField] public string prodApiKeyAndroid;
        [SerializeField] public string prodApiKeyDesktop;
        [SerializeField] public string devApiKeyWeb;
        [SerializeField] public string devApiKeyIos;
        [SerializeField] public string devApiKeyAndroid;
        [SerializeField] public string devApiKeyDesktop;
    }
}
