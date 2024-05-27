using UnityEngine;
using UnityEngine.UI;
using Treasure;
using System.Collections.Generic;

#if UNITY_IOS
using UnityEngine.iOS;
#endif

public class TDKRunner : MonoBehaviour
{
    private List<string> _navOptions = new List<string> { "Connect", "Identity", "Analytics", "Bridgeworld" };
    private Text _debugTxt;
    private Text _versionTxt;

    #region lifecycle
    void Awake()
    {
        // assign UI
        _debugTxt = GameObject.Find("Canvas/DebugPanel/ScrollView/Text").GetComponent<Text>();
        _versionTxt = GameObject.Find("Canvas/DebugPanel/VersionTxt").GetComponent<Text>();

        // set version
        _versionTxt.text = "v" + TDKVersion.version;

        TDK.Instance.PersistentDataPath = Application.persistentDataPath;

        #if UNITY_IOS
        // Disable iCloud backup for the persistent folder
        Device.SetNoBackupFlag(TDK.Instance.PersistentDataPath);
        #endif

        TDKLogger.ExternalLogCallback += DebugPanelLog;
    }

    void Start()
    {
        OnNavBtn(_navOptions[0]);
    }
    #endregion

    #region helpers
    public void DebugPanelLog(string message)
    {
        _debugTxt.text += "\n" + message;
    }

    private void OnNavBtn(string navName)
    {
        _navOptions.ForEach((navOption) =>
        {
            GameObject.Find("Canvas/UI/UI_Containers/" + navOption + "_Container").SetActive(navName == navOption);
            GameObject.Find("Canvas/UI/Nav/" + navOption + "_Btn").GetComponent<Button>().interactable = navName != navOption;
        });
    }

    public void OnClearDebugPanelBtn()
    {
        _debugTxt.text = "";
    }
    #endregion
}
