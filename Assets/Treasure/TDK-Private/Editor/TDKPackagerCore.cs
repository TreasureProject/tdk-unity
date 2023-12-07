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

            var contents = new List<string>();

            // Editor
            contents.Add("Assets/Treasure/TDK/Editor/TDKConfigEditor.cs");
            contents.Add("Assets/Treasure/TDK/Editor/TDKConfigWindow.cs");

            // Runtime
            contents.Add("Assets/Treasure/TDK/Runtime/Analytics");
            contents.Add("Assets/Treasure/TDK/Runtime/Identity");
            contents.Add("Assets/Treasure/TDK/Runtime/Infrastructure");
            contents.Add("Assets/Treasure/TDK/Runtime/TDK.cs");
            contents.Add("Assets/Treasure/TDK/Runtime/TDKConfig.cs");
            contents.Add("Assets/Treasure/TDK/Runtime/TDKVersion.cs");
            contents.Add("Assets/Treasure/TDK/Runtime/Utils");

            contents.Add("Assets/package.json");

            AssetDatabase.Refresh();

            AssetDatabase.ExportPackage(
                contents.ToArray(),
                string.Format("{0}_v{1}_{2}.unitypackage", TDKVersion.name, TDKVersion.version, _packageName.ToString()),
                ExportPackageOptions.Recurse | ExportPackageOptions.IncludeDependencies);

            Debug.Log(string.Format("TDK {0} Core Package exported", _packageName.ToString()));
        }
    }
}
