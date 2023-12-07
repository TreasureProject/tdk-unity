using UnityEngine;
using UnityEditor;
using System.IO;

namespace Treasure
{
    public partial class TDKConfigEditor
    {
        private static void CheckForAndCreateResourcesDir(string dir)
        {
            if(!Directory.Exists(dir))
			{
				Directory.CreateDirectory(dir);
			}
        }

        [MenuItem ("Treasure/TDK/Create Config/Root Config", false, 101)]
        public static void CreateTDKConfig_Menu()
        {
            CheckForAndCreateResourcesDir(TDKConfig.DEFAULT_CONFIG_LOCATION);
            var config = ScriptableObject.CreateInstance<TDKConfig>();
			AssetDatabase.CreateAsset(config, TDKConfig.DEFAULT_CONFIG_LOCATION + "/TDKConfig.asset");
        }

		public static void CreateTDKConfig(SerializedTDKConfig serializedConfig=null)
        {
            CheckForAndCreateResourcesDir(TDKConfig.DEFAULT_CONFIG_LOCATION);
			
            var config = ScriptableObject.CreateInstance<TDKConfig>();
			
			if(serializedConfig != null)
			{
				config.SetConfig(serializedConfig);
			}

			AssetDatabase.CreateAsset(config, TDKConfig.DEFAULT_CONFIG_LOCATION + "/TDKConfig.asset");
        }
    }
}
