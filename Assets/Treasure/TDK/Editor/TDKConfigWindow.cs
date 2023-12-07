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

        void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            {
                GUILayout.Label("Paste game config JSON below:");
                _jsonConfigStr = EditorGUILayout.TextArea(_jsonConfigStr, GUILayout.Height(495));

                if (GUILayout.Button("Cofigure TDK", GUILayout.ExpandWidth(true), GUILayout.Height(80)))
                {
                    ConfigTDK();
                }
            }
            EditorGUILayout.EndVertical();
        }

        public static void ConfigTDK()
        {
            var gameConfig = JsonUtility.FromJson<TDKGameConfig>(_jsonConfigStr);
            TDKConfigEditor.CreateTDKConfig(gameConfig.tdk);

            #if TDK_HELIKA
            TDKConfigEditor.CreateHelikaConfig(gameConfig.helika);
            #endif

            _window.Close();
        }
    }

    [Serializable]
    public class TDKGameConfig
    {
        [SerializeField] public SerializedTDKConfig tdk;
        
        #if TDK_HELIKA
        [SerializeField] public SerializedHelikaConfig helika;
        #endif
    }
}
