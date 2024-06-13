using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Treasure;
using System.Text.RegularExpressions;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using System.Text;
using System;

// For running these tests you need to modify scripting defines: remove TDK_HELIKA and add TREASURE_ANALYTICS
// It needs to be done manually for now, we might want a better way, perhaps something like this:
// https://forum.unity.com/threads/info-on-unity_include_tests-define.890095/#post-7495937
// to remove/add scripting defines programatically, perhaps a custom editor button can do that + run the tests

public class AnalyticsTest
{
    public class TestTDKAbstractedEngineApi : TDKAbstractedEngineApi
    {
        const string keyPrefix = "[unitTesting] ";

        public override string ApplicationPersistentDataPath()
        {
            return Path.Combine(base.ApplicationPersistentDataPath(), "Testing");
        }

        public override T GetPersistedValue<T>(string key)
        {
            return base.GetPersistedValue<T>(keyPrefix + key);
        }

        public override void SetPersistedValue<T>(string key, T value)
        {
            base.SetPersistedValue(keyPrefix + key, value);
        }

        public override void DeletePersistedValue(string key)
        {
            base.DeletePersistedValue(keyPrefix + key);
        }
    }

    public class MockHttpMessageHandler : HttpMessageHandler
    {
        public HttpStatusCode statusCode;
        public string jsonResponse;
        
        public MockHttpMessageHandler(HttpStatusCode statusCode, string jsonResponse)
        {
            this.statusCode = statusCode;
            this.jsonResponse = jsonResponse;
        }
        
        sealed protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        ) {
            Debug.Log($"Intercepted request to the following route: {request.RequestUri}");
            var httpResponse = new HttpResponseMessage() {
                StatusCode = statusCode,
                Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json"),
            };
            await Task.Delay(TimeSpan.FromSeconds(1));

            return httpResponse;
        }
    }

    void ClearPersistedEventBatches()
    {
        var path = Path.Combine(
            testTDKAbstractedEngineApi.ApplicationPersistentDataPath(),
            AnalyticsConstants.PERSISTENT_DIRECTORY_NAME
        );
        if (Directory.Exists(path))
        {
            var directoryInfo = new DirectoryInfo(path);

            var files = directoryInfo.GetFiles();

            foreach (var file in files)
            {
                file.Delete();
            }
        } else {
            Directory.CreateDirectory(path);
        }
    }

    TDKConfig testTDKConfig;
    TestTDKAbstractedEngineApi testTDKAbstractedEngineApi;

    [UnitySetUp]
    public IEnumerator MySetUp()
    {
        // skip initialization to avoid any noise from automatic requests + object creation
        TDK.skipAutoInitialize = true;
        
        testTDKConfig = ScriptableObject.CreateInstance<TDKConfig>();
        testTDKConfig.SetConfig(new SerializedTDKConfig {
            cartridgeTag = "Unit Testing",
            devTdkApiUrl = "https://localhost:5000/devTdkApiUrl",
            prodTdkApiUrl = "https://localhost:5000/prodTdkApiUrl",
            devAnalyticsApiUrl = "https://localhost:5000/devAnalyticsApiUrl",
            prodAnalyticsApiUrl = "https://localhost:5000/prodAnalyticsApiUrl",
            sessionLengthDays = 123
        });
        testTDKAbstractedEngineApi = new TestTDKAbstractedEngineApi();

        ClearPersistedEventBatches();
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
        // Debug.Log(testTDKAbstractedEngineApi.ApplicationPersistentDataPath());
        TDK.Instance.InitProperties(
            testTDKConfig,
            testTDKAbstractedEngineApi,
            new LocalSettings(testTDKAbstractedEngineApi.ApplicationPersistentDataPath())
        );
        TDK.Instance.InitAnalytics();
        LogAssert.Expect(
            LogType.Log, 
            "[TDKAnalyticsService.Cache:InitPersistentCache] _persistentFolderPath: " +
                Path.Combine(testTDKAbstractedEngineApi.ApplicationPersistentDataPath(), AnalyticsConstants.PERSISTENT_DIRECTORY_NAME)
        );
        TDKServiceLocator.GetService<TDKAnalyticsService>().SetHttpMssageHandler(
            new MockHttpMessageHandler(HttpStatusCode.OK, "{}")
        );
        TDK.Analytics.TrackCustomEvent("test event", null, highPriority: true);
        LogAssert.Expect(LogType.Log, $"Intercepted request to the following route: https://localhost:5000/devAnalyticsApiUrl/events");
        LogAssert.Expect(LogType.Log, "[TDKAnalyticsService.IO:SendEvents] Events sent successfully");
        yield return new WaitForSeconds(2);
    }

    // [UnityTest]
    // public IEnumerator AnalyticsTestThatFails()
    // {
    //     Assert.True(true);
    //     yield return null;
    //     Assert.True(false);
    // }
}
