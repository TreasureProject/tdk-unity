using System.Collections;
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

        TDK.Initialize(
            tdkConfig: TDKConfig.LoadFromResources(),
            testTDKAbstractedEngineApi,
            new LocalSettings(testTDKAbstractedEngineApi.ApplicationPersistentDataPath())
        );

        TestHelpers.ClearPersistedEventBatches(testTDKAbstractedEngineApi);
        
        yield return null;
    }

    [UnityTest]
    public IEnumerator Integration1()
    {
        SceneManager.LoadScene("Assets/Treasure/Example/Scenes/TDKHarness.unity", LoadSceneMode.Single);

        yield return new WaitForSeconds(1);

        yield return ConnectViaUI();
        
        yield return new WaitForSeconds(1);

        var navButtonAnalytics = GameObject.Find("Analytics_Btn");
        navButtonAnalytics.GetComponent<Button>().onClick.Invoke();

        var trackCustomEventButton = GameObject.Find("Btn_TrackCustomEvent");

        Assert.That(trackCustomEventButton.activeInHierarchy, Is.True);
        trackCustomEventButton.GetComponent<Button>().onClick.Invoke();
        
        yield return new WaitForSeconds(3);
    }

    private static IEnumerator ConnectViaUI()
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

        Assert.That(headerLogo.GetCurrentNameText(), Is.EqualTo("Zeeverse"));
        googleButton.GetComponent<Button>().onClick.Invoke();
        yield return TestHelpers.WaitUntilWithMax(() => headerLogo.GetCurrentNameText() != "Zeeverse", 10f);
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
    }
}