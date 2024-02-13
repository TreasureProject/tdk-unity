using UnityEngine;
using UnityEditor;

namespace Treasure
{
    public partial class TDKConfigEditor
    {
        [MenuItem ("Treasure/TDK/Create Config/Thirdweb", false, 103)]
        public static void CreateThirdwebConfig()
        {
            CheckForAndCreateResourcesDir(TDKConfig.DEFAULT_CONFIG_LOCATION);
            var config = ScriptableObject.CreateInstance<TDKThirdwebConfig>();
            AssetDatabase.CreateAsset(config, TDKConfig.DEFAULT_CONFIG_LOCATION + "/TDKThirdwebConfig.asset");

            var tdkConfig = TDKConfig.LoadFromResources();
            tdkConfig.SetModuleConfig<TDKThirdwebConfig>(config);
            EditorUtility.SetDirty(tdkConfig);
        }

        public static void CreateThirdwebConfig(SerializedThirdwebConfig serializedConfig=null)
        {
            CheckForAndCreateResourcesDir(TDKConfig.DEFAULT_CONFIG_LOCATION);

            var config = ScriptableObject.CreateInstance<TDKThirdwebConfig>();

            if(serializedConfig != null)
            {
                config.SetConfig(serializedConfig);
            }

            AssetDatabase.CreateAsset(config, TDKConfig.DEFAULT_CONFIG_LOCATION + "/TDKThirdwebConfig.asset");

            var tdkConfig = TDKConfig.LoadFromResources();
            tdkConfig.SetModuleConfig<TDKThirdwebConfig>(config);
            EditorUtility.SetDirty(tdkConfig);
        }
    }
}
