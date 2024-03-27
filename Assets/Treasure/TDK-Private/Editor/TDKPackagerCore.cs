using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Treasure
{
    public class TDKPackagerCore
    {
        private static string _packageName = "Core";

        [MenuItem("Treasure/TDK/Export Packages/Core", false, 800)]
        public static void ExportPackage()
        {
            Debug.Log(string.Format("Exporting TDK {0} Core Package...", _packageName.ToString()));

            var contents = new List<string>
            {
                // Example
                "Assets/Treasure/Example",

                // Editor
                "Assets/Treasure/TDK/Editor/TDK.Editor.asmdef",
                "Assets/Treasure/TDK/Editor/TDKConfigEditor.cs",
                "Assets/Treasure/TDK/Editor/TDKConfigWindow.cs",

                // Runtime
                "Assets/Treasure/TDK/Runtime/Analytics",
                "Assets/Treasure/TDK/Runtime/API",
                "Assets/Treasure/TDK/Runtime/Bridgeworld",
                "Assets/Treasure/TDK/Runtime/Common",
                "Assets/Treasure/TDK/Runtime/Identity",
                "Assets/Treasure/TDK/Runtime/Infrastructure",

                "Assets/Treasure/TDK/Runtime/Services/Treasure",
                
                "Assets/Treasure/TDK/Runtime/TDK.cs",
                "Assets/Treasure/TDK/Runtime/TDK.Runtime.asmdef",
                "Assets/Treasure/TDK/Runtime/TDKConfig.cs",
                "Assets/Treasure/TDK/Runtime/TDKVersion.cs",
                "Assets/Treasure/TDK/Runtime/Utils",

                // Resources
                "Assets/Treasure/TDK/Resources/TDKConnectSettings.asset",
                "Assets/Treasure/TDK/Resources/TDKConnectThemeData.asset",

                "Assets/package.json",

                
            };

            AssetDatabase.Refresh();

            AssetDatabase.ExportPackage(
                contents.ToArray(),
                string.Format("{0}_v{1}_{2}.unitypackage", TDKVersion.name, TDKVersion.version, _packageName.ToString()),
                ExportPackageOptions.Recurse);

            Debug.Log(string.Format("TDK {0} Core Package exported", _packageName.ToString()));
        }
    }
}
