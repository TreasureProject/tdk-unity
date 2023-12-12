using UnityEngine;
using System;

namespace Treasure
{
    public class TDKHelikaConfig : ScriptableObject
    {
        [SerializeField] private string _apiKey = string.Empty;
        [SerializeField] private string _env = string.Empty;

        public string ApiKey
        {
            get { return _apiKey; }
        }

        public string Env
        {
            get { return _env; }
        }

        public void SetConfig(SerializedHelikaConfig config)
        {
            _apiKey = config.apiKey;
            _env = config.env;
        }
    }

    [Serializable]
    public class SerializedHelikaConfig
    {
        [SerializeField] public string apiKey;
        [SerializeField] public string env;
    }
}
