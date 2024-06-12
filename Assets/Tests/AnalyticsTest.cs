using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Treasure;
using System.Text.RegularExpressions;
using System.IO;

// For running these tests you need to modify scripting defines: remove TDK_HELIKA and add TREASURE_ANALYTICS
// It needs to be done manually for now, we might want a better way, perhaps something like this:
// https://forum.unity.com/threads/info-on-unity_include_tests-define.890095/#post-7495937
// to remove/add scripting defines programatically, perhaps a custom editor button can do that + run the tests

public class AnalyticsTest
{
    // TODO do this in MySetUp, providing an AbstractedEngineApi that uses a test-only folder,
    // as to not affect the actual persisted files generated so far. MySetUp needs to ensure the folder exists
    void DeletePersistedEventBatches()
    {
        // needs to be after TDK.Instance.InitProperties for `path` to be defined properly
        var path = Path.Combine(
            TDK.Instance.AbstractedEngineApi.ApplicationPersistentDataPath(),
            AnalyticsConstants.PERSISTENT_DIRECTORY_NAME
        );
        var directoryInfo = new DirectoryInfo(path);

        var files = directoryInfo.GetFiles();

        foreach (var file in files)
        {
            file.Delete();
        }
    }

    [UnitySetUp]
    public IEnumerator MySetUp()
    {
        // skip initialization to avoid any noise from automatic requests + object creation
        TDK.skipAutoInitialize = true;
        yield return null;
    }

    [TearDown]
    public void MyTearDown()
    {

    }

    // TODO figure out how to make this test pass again if repeated
    [UnityTest]
    [TestMustExpectAllLogs]
    public IEnumerator AnalyticsTestInit()
    {
        yield return new WaitUntil(() => TDK.Initialized);
        TDK.Instance.InitProperties(
            TDKConfig.LoadFromResources(), // TODO replace
            new TDKAbstractedEngineApi(),
            new LocalSettings(Application.persistentDataPath)
        );
        DeletePersistedEventBatches();
        TDK.Instance.InitAnalytics();
        LogAssert.Expect(LogType.Log, new Regex($"{Regex.Escape("[TDKAnalyticsService.Cache:InitPersistentCache] _persistentFolderPath:")} .+"));
        yield return null; // wait for next frame in case gameObjects need to be created
        TDK.Analytics.TrackCustomEvent("test event", null, highPriority: true);
        // TODO mock http client (optionally, having integration tests would be nice too)
        LogAssert.Expect(LogType.Warning, "[TDKAnalyticsService.IO:SendEvents] Failed to send events: Invalid URI: The hostname could not be parsed.");
        yield return new WaitForSeconds(2);
        // TODO expect 0 events in queue
        // Assert.AreEqual(TDKServiceLocator.GetService<TDKAnalyticsService>()._memoryCache.Count, 0);
        // LogAssert.Expect(LogType.Log, new Regex("AAA"));
    }

    // [UnityTest]
    // public IEnumerator AnalyticsTestThatFails()
    // {
    //     Assert.True(true);
    //     yield return null;
    //     Assert.True(false);
    // }
}
