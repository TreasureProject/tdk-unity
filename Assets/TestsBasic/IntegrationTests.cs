using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Treasure;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class IntegrationTests
{
    [UnitySetUp]
    public IEnumerator MySetUp()
    {
        var testTDKAbstractedEngineApi = TestHelpers.GetTestAbstractedEngineApi();
        var tdkConfig = TDKConfig.LoadFromResources();
        Assert.That(tdkConfig.AutoInitialize, Is.False, "TDKConfig AutoInitialize must be false when testing");

        tdkConfig.Environment = TDKConfig.Env.DEV;
        tdkConfig.LoggerLevel = TDKConfig.LoggerLevelValue.DEBUG;
        TDK.Initialize(
            tdkConfig: tdkConfig,
            testTDKAbstractedEngineApi,
            new LocalSettings(testTDKAbstractedEngineApi.ApplicationPersistentDataPath())
        );

        TestHelpers.ClearPersistedEventBatches(testTDKAbstractedEngineApi);
        
        yield return null;
    }

    List<string> tdkLogs;
    bool connected = false;

    [UnityTest]
    public IEnumerator Integration1()
    {
        tdkLogs = new List<string>();
        TDKLogger.ExternalLogCallback += (msg) => { tdkLogs.Add(msg); };
        
        SceneManager.LoadScene("Assets/Treasure/Example/Scenes/TDKHarness.unity", LoadSceneMode.Single);

        yield return new WaitForSeconds(1);

        yield return ConnectViaUI();
        
        yield return new WaitForSeconds(1);

        if (connected) {
            yield return SetChain();
        }

        yield return SendAnalyticsEvents();
        
        yield return new WaitForSeconds(3);
    }

    private IEnumerator SetChain()
    {
        _ = TDK.Connect.SetChainId(ChainId.Arbitrum);
        yield return new WaitForSeconds(3);
        _ = TDK.Connect.SetChainId(ChainId.ArbitrumSepolia);
        yield return new WaitForSeconds(3);
        _ = TDK.Connect.SetChainId(ChainId.ArbitrumSepolia);
        yield return new WaitForSeconds(3);
    }

    // Note: this does not work on webgl by default, for socials login (oauth) we need a host with cors enabled
    private IEnumerator ConnectViaUI()
    {
        var navButtonConnect = GameObject.Find("Connect_Btn");
        navButtonConnect.GetComponent<Button>().onClick.Invoke();

        var connectButton = GameObject.Find("Btn_ConnectWallet");
        var loginModal = Object.FindAnyObjectByType<LoginModal>(FindObjectsInactive.Include);

        Assert.That(connectButton.activeInHierarchy, Is.True);
        Assert.That(loginModal.gameObject.activeInHierarchy, Is.False);
        connectButton.GetComponent<Button>().onClick.Invoke();
        yield return null;
        Assert.That(loginModal.gameObject.activeInHierarchy, Is.True);

        var accountModal = Object.FindAnyObjectByType<AccountModal>(FindObjectsInactive.Include);
        var headerLogo = Object.FindAnyObjectByType<HeaderLogo>();
        var googleButton = GameObject.Find("ButtonFrameIcon(Google)");
        var backgroundButton = GameObject.Find("TransparentOverlayButton");

        Assert.That(headerLogo.GetCurrentNameText(), Is.EqualTo("Loading..."));
        googleButton.GetComponent<Button>().onClick.Invoke();
        yield return TestHelpers.WaitUntilWithMax(() => headerLogo.GetCurrentNameText() != "Loading...", 10f);
        Assert.That(headerLogo.GetCurrentNameText(), Is.EqualTo("TDK Harness"));
        yield return TestHelpers.WaitUntilWithMax(() => TDK.Connect.Address != null, 30f);
        Assert.That(loginModal.gameObject.activeInHierarchy, Is.False);
        Assert.That(accountModal.gameObject.activeInHierarchy, Is.False);

        yield return new WaitForSeconds(1);

        connectButton.GetComponent<Button>().onClick.Invoke();
        Assert.That(loginModal.gameObject.activeInHierarchy, Is.False);
        Assert.That(accountModal.gameObject.activeInHierarchy, Is.True);
        Assert.That(accountModal.GetAddressText(), Does.StartWith(TDK.Connect.Address[..6]));
        yield return new WaitForSeconds(3);
        backgroundButton.GetComponent<Button>().onClick.Invoke();
        Assert.That(accountModal.gameObject.activeInHierarchy, Is.False);
        
        connected = true;
    }

    private IEnumerator SendAnalyticsEvents()
    {
        var navButtonAnalytics = GameObject.Find("Analytics_Btn");
        navButtonAnalytics.GetComponent<Button>().onClick.Invoke();

        var trackCustomEventButton = GameObject.Find("Btn_TrackCustomEvent");

        Assert.That(trackCustomEventButton.activeInHierarchy, Is.True);
        
        tdkLogs.Clear();
        // TODO force flush events
        trackCustomEventButton.GetComponent<Button>().onClick.Invoke();

        yield return TestHelpers.WaitUntilWithMax(() => tdkLogs.Count >= 2, 15f);

        Assert.That(tdkLogs.Count, Is.EqualTo(2));
        
        var payloadLogParts = tdkLogs[0].Split(" Payload:");
        Assert.That(payloadLogParts[0], Is.EqualTo("[TDKAnalyticsService.IO:SendEventBatch]"));
        var eventsArray = Newtonsoft.Json.JsonConvert.DeserializeObject<List<AnalyticsEvent>>(payloadLogParts[1])!;
        Assert.That(eventsArray[eventsArray.Count - 1].name, Is.EqualTo("custom_event"));
        
        Assert.That(tdkLogs[1], Is.EqualTo("[TDKAnalyticsService.IO:SendEvents] Events sent successfully"));
    }

    [System.Serializable]
    class AnalyticsEvent {
        public string name;
    }
}