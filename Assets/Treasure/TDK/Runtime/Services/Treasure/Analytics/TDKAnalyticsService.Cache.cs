using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

#if UNITY_IOS
using UnityEngine.iOS;
#endif

namespace Treasure
{
    public partial class TDKAnalyticsService : TDKBaseService
    {
        private string persistentFolderPath;
        private CancellationTokenSource cancellationTokenSource;

        private void InitPersistentCache()
        {
            // build directory
            this.persistentFolderPath = Path.Combine(Application.persistentDataPath, AnalyticsConstants.PERSISTENT_DIRECTORY_NAME);
            TDKLogger.Log("[TDKAnalyticsService.Cache:InitPersistentCache] persistentFolderPath: " + persistentFolderPath);
            
            // ensure directory exists
            if (!Directory.Exists(persistentFolderPath))
            {
                Directory.CreateDirectory(persistentFolderPath);
            }

#if UNITY_IOS
            // set no [icloud] backup 
            Device.SetNoBackupFlag(persistentFolderPath);
#endif
            // init threadpool 
            cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            // start a new thread for periodic file checking
            ThreadPool.QueueUserWorkItem((state) =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    ProcessFiles();
                    Thread.Sleep((int)(AnalyticsConstants.PERSISTENT_CHECK_INTERVAL_SECONDS * 1000)); // Convert seconds to milliseconds
                }
            });
        }

        private void ProcessFiles()
        {
            string[] files = Directory.GetFiles(persistentFolderPath, "*.eventbatch"); // Get all txt files in directory

            foreach (string filePath in files)
            {
                TDKLogger.Log("[TDKAnalyticsService.Cache:ProcessFiles] processing: " + filePath);
                string content = File.ReadAllText(filePath); // Read file content
                
                TDKMainThreadDispatcher.Instance.Enqueue(SendPersistedEventBatchRoutine(content, filePath));
            }
        }

        private async void PersistEventBatchAsync(List<string> events)
        {
            await Task.Run(() =>
            {
                string payload = string.Join(",", events);

                // simulate writing the payload to disk
                var fileGuid = Guid.NewGuid().ToString("N");
                string filePath = Path.Combine(persistentFolderPath, $"tdk_{fileGuid}.eventbatch");
                File.WriteAllText(filePath, payload);
                TDKLogger.Log("[TDKAnalyticsService.Cache:PersistPayloadToDiskAsync] Payload persisted to disk: " + filePath);
            });
        }
    }
}
