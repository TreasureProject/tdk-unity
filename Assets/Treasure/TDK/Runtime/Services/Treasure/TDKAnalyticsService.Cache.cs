using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
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

        private async void ProcessFiles()
        {
            string[] files = Directory.GetFiles(persistentFolderPath, "*.eventbatch"); // Get all txt files in directory

            foreach (string filePath in files)
            {
                string content = File.ReadAllText(filePath); // Read file content
                bool success = await ProcessContent(content);

                if (success)
                {
                    TDKLogger.Log("[TDKAnalyticsService.Cache:ProcessFiles] File processed successfully: " + filePath);
                    File.Delete(filePath); // Delete file after successful processing
                }
                else
                {
                    TDKLogger.Log("[TDKAnalyticsService.Cache:ProcessFiles] File processing failed: " + filePath);
                }
            }
        }

        private async Task<bool> ProcessContent(string content)
        {
            int retries = 0;

            while (retries < AnalyticsConstants.PERSISTENT_MAX_RETRIES)
            {
                // attemps re-send
                bool processingSuccess = await RetrySendEvents(content);

                if (processingSuccess) {
                    return true;
                }

                retries++;
            }

            return false;
        }

        private async void PersistPayloadToDiskAsync(string payload)
        {
            await Task.Run(() =>
            {
                // simulate writing the payload to disk
                var fileGuid = Guid.NewGuid().ToString("N");
                string filePath = Path.Combine(persistentFolderPath, $"{fileGuid}.eventbatch");
                File.WriteAllText(filePath, payload);
                TDKLogger.Log("[TDKAnalyticsService.Cache:PersistPayloadToDiskAsync] Payload persisted to disk: " + filePath);
            });
        }
    }
}
