using UnityEngine;
using System;

namespace Treasure
{
    public class TDKHelikaConfig : ScriptableObject
    {
        [SerializeField] public string _prodApiKeyWeb;
        [SerializeField] public string _prodApiKeyIos;
        [SerializeField] public string _prodApiKeyAndroid;
        [SerializeField] public string _devApiKeyWeb;
        [SerializeField] public string _devApiKeyIos;
        [SerializeField] public string _devApiKeyAndroid;

        public string ApiKey
        {
            get {
                if(TDK.Instance.AppConfig.Environment == TDKConfig.Env.PROD) {
                    #if UNITY_IOS
                    return _prodApiKeyIos;
                    #elif UNITY_ANDROID
                    return _prodApiKeyAndroid;
                    #else
                    return _prodApiKeyWeb;
                    #endif
                }
                else {
                    #if UNITY_IOS
                    return _devApiKeyIos;
                    #elif UNITY_ANDROID
                    return _devApiKeyAndroid;
                    #else
                    return _devApiKeyWeb;
                    #endif
                }
            }
        }

        public void SetConfig(SerializedHelikaConfig config)
        {
            _prodApiKeyWeb = config.prodApiKeyWeb;
            _prodApiKeyIos = config.prodApiKeyIos;
            _prodApiKeyAndroid = config.prodApiKeyAndroid;
            _devApiKeyWeb = config.devApiKeyWeb;
            _devApiKeyIos = config.devApiKeyIos;
            _devApiKeyAndroid = config.devApiKeyAndroid;
        }
    }

    [Serializable]
    public class SerializedHelikaConfig
    {
        [SerializeField] public string prodApiKeyWeb;
        [SerializeField] public string prodApiKeyIos;
        [SerializeField] public string prodApiKeyAndroid;
        [SerializeField] public string devApiKeyWeb;
        [SerializeField] public string devApiKeyIos;
        [SerializeField] public string devApiKeyAndroid;
    }
}
