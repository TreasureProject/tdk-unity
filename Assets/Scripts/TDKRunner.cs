using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

using Treasure;
using Nethereum.Contracts.Standards.ENS.PublicResolver.ContractDefinition;

public class TDKRunner : MonoBehaviour
{
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

        TDKLogger.ExternalLogCallback += DebugPanelLog;
    }

    void Start()
    {
        // enable core nav at start
        OnNavBtn("Identity");

        // Application.logMessageReceived += HandleLog;
    }
    #endregion

    #region helpers
    public void DebugPanelLog(string message)
    {
        _debugTxt.text += "\n" + message;
    }

    private void ResetNav()
    {
        GameObject.Find("Canvas/UI/UI_Containers/Identity_Container").SetActive(false);
        GameObject.Find("Canvas/UI/UI_Containers/Analytics_Container").SetActive(false);
        GameObject.Find("Canvas/UI/UI_Containers/Harvester_Container").SetActive(false);
        GameObject.Find("Canvas/UI/UI_Containers/Core_Container").SetActive(false);

        GameObject.Find("Canvas/UI/Nav/Identity_Btn").GetComponent<Button>().interactable = true;
        GameObject.Find("Canvas/UI/Nav/Analytics_Btn").GetComponent<Button>().interactable = true;
        GameObject.Find("Canvas/UI/Nav/Harvester_Btn").GetComponent<Button>().interactable = true;
        GameObject.Find("Canvas/UI/Nav/Core_Btn").GetComponent<Button>().interactable = true;
    }

    private void OnNavBtn(string navName)
    {
        ResetNav();

        GameObject.Find("Canvas/UI/UI_Containers/" + navName + "_Container").SetActive(true);
        GameObject.Find("Canvas/UI/Nav/" + navName + "_Btn").GetComponent<Button>().interactable = false;
    }

    public void OnClearDebugPanelBtn()
    {
        _debugTxt.text = "";
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        DebugPanelLog(logString);
    }
    #endregion
}
