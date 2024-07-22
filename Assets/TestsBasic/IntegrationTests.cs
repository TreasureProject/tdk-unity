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
            yield return SetChain(); // TODO this breaks the CreateSession call, is it intended?
        }

        yield return SendAnalyticsEvents();

        yield return new WaitForSeconds(3);

        yield return CreateSession();
        
        yield return ForceFlushCache();
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

    private IEnumerator SetChain()
    {
        yield return ForceFlushCache();
        
        tdkLogs.Clear();
        
        _ = TDK.Connect.SetChainId(ChainId.ArbitrumSepolia);
        yield return new WaitForSeconds(2);
        _ = TDK.Connect.SetChainId(ChainId.Arbitrum);
        yield return new WaitForSeconds(2);
        _ = TDK.Connect.SetChainId(ChainId.Arbitrum);
        yield return new WaitForSeconds(2);
        _ = TDK.Connect.SetChainId(ChainId.ArbitrumSepolia);
        yield return new WaitForSeconds(2);
        
        Assert.That(tdkLogs, Is.EquivalentTo(new List<string> {
            "Chain is already set to ArbitrumSepolia",
            "Initializing Thirdweb SDK for chain: arbitrum",
            "Switched chain to Arbitrum",
            "Chain is already set to Arbitrum",
            "Initializing Thirdweb SDK for chain: arbitrum-sepolia",
            "Switched chain to ArbitrumSepolia",
        }));
    }

    private IEnumerator SendAnalyticsEvents()
    {
        var navButtonAnalytics = GameObject.Find("Analytics_Btn");
        navButtonAnalytics.GetComponent<Button>().onClick.Invoke();

        var trackCustomEventButton = GameObject.Find("Btn_TrackCustomEvent");

        Assert.That(trackCustomEventButton.activeInHierarchy, Is.True);
        
        yield return ForceFlushCache();
        
        tdkLogs.Clear();
        trackCustomEventButton.GetComponent<Button>().onClick.Invoke();

        yield return TestHelpers.WaitUntilWithMax(() => tdkLogs.Count >= 2, 15f);

        Assert.That(tdkLogs.Count, Is.EqualTo(2));
        
        var eventsArray = AnalyticsEvent.ExtractPayloadFromLog(tdkLogs[0]);
        Assert.That(eventsArray.Count, Is.EqualTo(1));
        Assert.That(eventsArray[0].name, Is.EqualTo("custom_event"));
        Assert.That(tdkLogs[1], Is.EqualTo("[TDKAnalyticsService.IO:SendEvents] Events sent successfully"));
    }

    private IEnumerator CreateSession()
    {   
        yield return ForceFlushCache();

        tdkLogs.Clear();

        Assert.That(TDK.Identity.IsAuthenticated, Is.False);
        var startTask = TDK.Identity.StartUserSession();
        yield return TestHelpers.WaitUntilWithMax(() => startTask.IsCompleted, 10);
        Assert.That(TDK.Identity.IsAuthenticated, Is.True);

        Assert.That(tdkLogs.Count, Is.EqualTo(5));
        Assert.That(tdkLogs[0], Is.EqualTo("Fetching login payload"));
        Assert.That(tdkLogs[1], Is.EqualTo("Signing login payload"));
        Assert.That(tdkLogs[2], Is.EqualTo("Logging in and fetching TDK auth token"));
        Assert.That(tdkLogs[3], Is.EqualTo("Using existing session key").Or.EqualTo("Creating new session key"));
        Assert.That(tdkLogs[4], Is.EqualTo("User session started successfully"));

        tdkLogs.Clear();

        startTask = TDK.Identity.StartUserSession();
        yield return TestHelpers.WaitUntilWithMax(() => startTask.IsCompleted, 10);

        Assert.That(tdkLogs, Is.EquivalentTo(new List<string> {
            "Validating existing user session",
            "Fetching user details",
            "Existing user session is valid",
            "User already has a valid session",
        }));

        tdkLogs.Clear();

        var endTask = TDK.Identity.EndUserSession();
        yield return TestHelpers.WaitUntilWithMax(() => endTask.IsCompleted, 10);
        Assert.That(TDK.Identity.IsAuthenticated, Is.False);

        yield return ForceFlushCache();

        Assert.That(tdkLogs.Count, Is.EqualTo(2));
        
        var eventsArray = AnalyticsEvent.ExtractPayloadFromLog(tdkLogs[0]);
        Assert.That(eventsArray.Count, Is.EqualTo(1));
        Assert.That(eventsArray[0].name, Is.EqualTo("tc_disconnected"));
        Assert.That(tdkLogs[1], Is.EqualTo("[TDKAnalyticsService.IO:SendEvents] Events sent successfully"));
    }

    private IEnumerator ForceFlushCache()
    {
        var analyticsService = TDKServiceLocator.GetService<TDKAnalyticsService>();
        analyticsService.FlushCache();
        yield return new WaitForSeconds(5);
    }

    [System.Serializable]
    class AnalyticsEvent {
        public string name;

        public static List<AnalyticsEvent> ExtractPayloadFromLog(string logString) {
            var payloadLogParts = logString.Split(" Payload:");
            Assert.That(payloadLogParts[0], Is.EqualTo("[TDKAnalyticsService.IO:SendEventBatch]"));
            var eventsArray = Newtonsoft.Json.JsonConvert.DeserializeObject<List<AnalyticsEvent>>(payloadLogParts[1])!;
            return eventsArray;
        }
    }
}