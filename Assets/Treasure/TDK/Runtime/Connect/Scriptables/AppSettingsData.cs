using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Treasure
{
    [CreateAssetMenu(fileName = "AppSettingsData", menuName = "ScriptableObjects/AppSettingsData")]
    public class AppSettingsData : ScriptableObject
    {
        [Serializable]
        public class ChainSettings
        {
            public ChainId chainId;
            public string backendWallet;
            public List<string> callTargets;
        }

        public Sprite icon;
        public string title;
        [Space]
        [SerializeField] private List<ChainSettings> chainSettingsMap = new();
        public LoginSettings loginSettings;

        public static AppSettingsData LoadFromResources()
        {
            return Resources.Load<AppSettingsData>("AppSettingsData");
        }

        public async Task<string> GetBackendWallet()
        {
            ChainId chainId = await TDK.Connect.GetChainId();
            ChainSettings chainSettings = chainSettingsMap.Find(d => d.chainId == chainId);
            return chainSettings?.backendWallet.ToLowerInvariant();
            
        }

        public async Task<List<string>> GetCallTargets()
        {
            ChainId chainId = await TDK.Connect.GetChainId();
            ChainSettings chainSettings = chainSettingsMap.Find(d => d.chainId == chainId);
            return chainSettings != null ? chainSettings.callTargets.ConvertAll(ct => ct.ToLowerInvariant()) : new List<string>();
        }
    }
}
