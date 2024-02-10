using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Treasure
{
    public class TDKPackagerThirdweb
    {
        private static string _packageName = "Thirdweb";

        [MenuItem("Treasure/TDK/Export Packages/Service-Thirdweb", false, 800)]
        public static void ExportPackage()
        {
            Debug.Log($"Exporting TDK {_packageName} Module Package...");

            var contents = new List<string>
            {
                $"Assets/{_packageName}",
                $"Assets/WebGLTemplates/{_packageName}",
                $"Assets/Treasure/TDK/Editor/Services/{_packageName}",
                $"Assets/Treasure/TDK/Runtime/Services/{_packageName}"
            };

            AssetDatabase.Refresh();

            AssetDatabase.ExportPackage(
                contents.ToArray(),
                string.Format("{0}_v{1}_{2}.unitypackage", TDKVersion.name, TDKVersion.version, _packageName.ToString()),
                ExportPackageOptions.Recurse | ExportPackageOptions.IncludeDependencies);

            Debug.Log($"TDK {_packageName} Module Package exported");
        }
    }
}