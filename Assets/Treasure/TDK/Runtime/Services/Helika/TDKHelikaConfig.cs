using UnityEngine;

using System;

namespace Treasure
{
    public class TDKHelikaConfig : ScriptableObject
    {
        [SerializeField] private string _apiKey = string.Empty;

        public string ApiKey
        {
            get { return _apiKey; }
        }

        public void SetConfig(SerializedHelikaConfig config)
        {
            _apiKey = config.apiKey;
        }
    }

    [Serializable]
    public class SerializedHelikaConfig
    {
        [SerializeField] public string apiKey;
    }
}
