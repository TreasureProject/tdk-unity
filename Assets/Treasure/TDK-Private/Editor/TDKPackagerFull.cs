using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Treasure
{
    public class TDKPackagerFull
    {
        private static string _packageName = "Full";

        [MenuItem("Treasure/TDK/Export Packages/Full", false, 800)]
        public static void ExportPackage()
        {
            Debug.Log($"Exporting TDK {_packageName.ToString()} Package...");

            var contents = new List<string>
            {
                // Plugins
                "Assets/Plugins",

                // 3rd party
                "Assets/Thirdweb",

                // Treasure
                "Assets/Treasure/Example",
                "Assets/Treasure/TDK/ConnectInternal",
                "Assets/Treasure/TDK/ConnectPrefabs",
                "Assets/Treasure/TDK/Editor",
                "Assets/Treasure/TDK/Resources/TDKConnectThemeData.asset",
                "Assets/Treasure/TDK/Runtime",

                // code stripping & compilation defines
                "Assets/link.xml",
                "Assets/csc.rsp"
            };

            AssetDatabase.Refresh();

            AssetDatabase.ExportPackage(
                contents.ToArray(),
                $"{TDKVersion.name}_v{TDKVersion.version}_{_packageName.ToString()}.unitypackage",
                ExportPackageOptions.Recurse);

            Debug.Log($"TDK {_packageName.ToString()} Package exported");
        }
    }
}
