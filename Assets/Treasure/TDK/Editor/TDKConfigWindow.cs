using UnityEngine;
using UnityEditor;
using System;

namespace Treasure
{
    public class TDKConfigWindow : EditorWindow
    {
        private static string _jsonConfigStr = string.Empty;

        private static EditorWindow _window;

        [MenuItem ("Treasure/TDK/Config", false, 100)]
        public static void ShowEditor()
        {
            _window = EditorWindow.GetWindow(typeof (TDKConfigWindow));
            _window.titleContent = new GUIContent("TDK Config");
            _window.minSize = new Vector2(450, 600);
        }

        [MenuItem ("Treasure/Set Dev Environment", false, 101)]
        public static void SetDevEnvironment() { SetDevEnvironment(true); }

        [MenuItem ("Treasure/Set Prod Environment", false, 102)]
        public static void SetProdEnvironment() { SetDevEnvironment(false); }

        [MenuItem ("Treasure/Edit Config", false, 103)]
        public static void EditConfig() {
            string tdkConfigPath = TDKConfig.DEFAULT_CONFIG_LOCATION + "/TDKConfig.asset";
            Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(tdkConfigPath);
        }

        void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            {
                GUILayout.Label("Paste game config JSON below:");
                _jsonConfigStr = EditorGUILayout.TextArea(_jsonConfigStr, GUILayout.Height(495));

                if (GUILayout.Button("Configure TDK", GUILayout.ExpandWidth(true), GUILayout.Height(80)))
                {
                    ConfigTDK();
                }
            }
            EditorGUILayout.EndVertical();
        }

        public static void ConfigTDK()
        {
            var gameConfig = JsonUtility.FromJson<SerializedTDKConfig>(_jsonConfigStr);
            TDKConfigEditor.CreateTDKConfig(gameConfig);

            _window.Close();
        }

        public static void SetDevEnvironment(bool devEnvironment)
        {
            try
            {
                TDKConfig config = TDKConfig.LoadFromResources();

                if(config == null)
                {
                    Debug.LogError("TDK - Could not load configuration file");
                }

                config.Environment = devEnvironment ? TDKConfig.Env.DEV : TDKConfig.Env.PROD;
                EditorUtility.SetDirty(config);
            }
            catch (System.Exception)
            {
                EditorUtility.DisplayDialog("Problem toggeling environment", "Please configure first.", "Ok");
                throw;
            }
        }
    }
}
