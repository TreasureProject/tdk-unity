

using System;
using System.Collections;
using System.IO;
using Treasure;
using UnityEngine;

public class TestHelpers
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

    public static TestTDKAbstractedEngineApi GetTestAbstractedEngineApi() {
        return new TestTDKAbstractedEngineApi();
    }

    public static void ClearPersistedEventBatches(TDKAbstractedEngineApi testTDKAbstractedEngineApi)
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

    public static IEnumerator WaitUntilWithMax(Func<bool> predicate, float maxWait) {
        float timeout = maxWait;
        yield return new WaitUntil(() => {
            timeout -= Time.deltaTime;
            return timeout <= 0 || predicate();
        });
    }
}