using UnityEngine;
using UnityEditor;

namespace Treasure
{
    public partial class TDKConfigEditor
    {
        [MenuItem ("Treasure/TDK/Create Config/Helika", false, 103)]
        public static void CreateHelikaConfig()
        {
            CheckForAndCreateResourcesDir(TDKConfig.DEFAULT_CONFIG_LOCATION);
            var config = ScriptableObject.CreateInstance<TDKHelikaConfig>();
            AssetDatabase.CreateAsset(config, TDKConfig.DEFAULT_CONFIG_LOCATION + "/TDKHelikaConfig.asset");

            var tdkConfig = TDKConfig.LoadFromResources();
            tdkConfig.SetModuleConfig<TDKHelikaConfig>(config);
            EditorUtility.SetDirty(tdkConfig);
        }

        public static void CreateHelikaConfig(SerializedHelikaConfig serializedConfig=null)
        {
            CheckForAndCreateResourcesDir(TDKConfig.DEFAULT_CONFIG_LOCATION);

            var config = ScriptableObject.CreateInstance<TDKHelikaConfig>();

            if(serializedConfig != null)
            {
                config.SetConfig(serializedConfig);
            }

            AssetDatabase.CreateAsset(config, TDKConfig.DEFAULT_CONFIG_LOCATION + "/TDKHelikaConfig.asset");

            var tdkConfig = TDKConfig.LoadFromResources();
            tdkConfig.SetModuleConfig<TDKHelikaConfig>(config);
            EditorUtility.SetDirty(tdkConfig);
        }
    }
}
