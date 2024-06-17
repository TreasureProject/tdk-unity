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
using System.Linq;

// For running these tests you need to modify scripting defines: remove TDK_HELIKA and add TREASURE_ANALYTICS
// It needs to be done manually for now, we might want a better way, perhaps something like this:
// https://forum.unity.com/threads/info-on-unity_include_tests-define.890095/#post-7495937
// to remove/add scripting defines programatically, perhaps a custom editor button can do that + run the tests

// Pending:
// - integration tests (with actual api calls)
// - mock unityrequest for webgl
// - figure out how to make test run consecutively. for now they must be run one by one
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
        public bool used = false;
        
        public MockHttpMessageHandler(HttpStatusCode statusCode, string jsonResponse)
        {
            this.statusCode = statusCode;
            this.jsonResponse = jsonResponse;
        }
        
        sealed protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        ) {
            if (used) {
                throw new Exception("MockHttpMessageHandler already used. Set a new one.");
            }
            used = true;
            var payload = await request.Content.ReadAsStringAsync();
            Debug.Log($"Intercepted request to the following route: {request.RequestUri} - payload: {payload}");
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

    void MockAnalyticsRequest(HttpStatusCode statusCode, string jsonResponse) {
        TDKServiceLocator.GetService<TDKAnalyticsService>().SetHttpMessageHandler(
            new MockHttpMessageHandler(statusCode, jsonResponse)
        );
    }

    System.Collections.Generic.List<string> logs = new();
    int validatedLogIndex = 0;
    string noValidatePrefix = "debugTestLog: ";

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        if (logString.StartsWith(noValidatePrefix)) return;
        DebugTestLog("adding test log");
        logs.Add(logString);
    }

    // use this for printing stuff in tests but not fail the tests
    void DebugTestLog(object message) {
        Debug.Log($"{noValidatePrefix}{message}");
    }

    void ValidateLogs(string[] logList) {
        for (int i = 0; i < logList.Length; i++)
        {
            var expectedLog = logList[i];
            ValidateNextLog(expectedLog);
        }
    }

    void ValidateNextLog(string expectedLog) {
        Assert.That(logs, Is.Not.Empty, "Not enough logs to validate");
        
        var logIndex = validatedLogIndex;
        var actualLog = logs[logIndex];
        
        if (expectedLog.StartsWith("rgx:")) {
            Assert.That(actualLog, Does.Match(expectedLog[4..]), $"Log does not match (index={logIndex})");
        } else {
            Assert.That(actualLog, Is.EqualTo(expectedLog), $"Log does not match (index={logIndex})");
        }
        validatedLogIndex += 1;
    }

    void EnsureAllLogsWereValidated() {
        Assert.That(logs.Count, Is.EqualTo(validatedLogIndex));
    }

    TDKConfig testTDKConfig;
    TestTDKAbstractedEngineApi testTDKAbstractedEngineApi;

    [UnitySetUp]
    public IEnumerator MySetUp()
    {
        Application.logMessageReceivedThreaded += HandleLog;
        logs.Clear();
        
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
        Application.logMessageReceivedThreaded -= HandleLog;
        EnsureAllLogsWereValidated();
    }

    [UnityTest]
    public IEnumerator AnalyticsTestComplex1()
    {
        TDK.Instance.InitializeProperties(
            testTDKConfig,
            testTDKAbstractedEngineApi,
            new LocalSettings(testTDKAbstractedEngineApi.ApplicationPersistentDataPath())
        );
        TDK.Instance.InitializeSubsystems();

        yield return TestHighPrioEvent();
        yield return TestBatchEvents();
    }

    IEnumerator TestHighPrioEvent() {
        MockAnalyticsRequest(HttpStatusCode.OK, "{}");
        
        string testEventName = "test event";
        TDK.Analytics.TrackCustomEvent(testEventName, null, highPriority: true);
        yield return new WaitForSeconds(2);
        
        string expectedPersistancePath = Path.Combine(testTDKAbstractedEngineApi.ApplicationPersistentDataPath(), AnalyticsConstants.PERSISTENT_DIRECTORY_NAME);
        string expectedRoute = "https://localhost:5000/devAnalyticsApiUrl/events";
        string expectedPayload = $".*\"name\":\"{testEventName}\".*";
        ValidateNextLog($"[TDKAnalyticsService.Cache:InitPersistentCache] _persistentFolderPath: {expectedPersistancePath}");
        ValidateNextLog(
            $"rgx:{Regex.Escape("Intercepted request to the following route: " + expectedRoute)} - payload: {expectedPayload}"
        );
        ValidateNextLog("[TDKAnalyticsService.IO:SendEvents] Events sent successfully");
    }

    IEnumerator TestBatchEvents() {
        MockAnalyticsRequest(HttpStatusCode.OK, "{}");

        string testEventName2 = "test event 2";
        TDK.Analytics.TrackCustomEvent(testEventName2, null);
        EnsureAllLogsWereValidated();
        yield return new WaitForSeconds(2);
        EnsureAllLogsWereValidated();
        yield return new WaitForSeconds(10);
        string expectedRoute = "https://localhost:5000/devAnalyticsApiUrl/events";
        string expectedPayload = $".*\"name\":\"{testEventName2}\".*";
        ValidateNextLog(
            $"rgx:{Regex.Escape("Intercepted request to the following route: " + expectedRoute)} - payload: {expectedPayload}"
        );
        ValidateNextLog("[TDKAnalyticsService.IO:SendEvents] Events sent successfully");
    }
    
    // TODO test harness scene and pressing buttons
}
