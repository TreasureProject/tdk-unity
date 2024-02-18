using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Threading.Tasks;

#if UNITY_IOS
using UnityEngine.iOS;
#endif

namespace Treasure
{
    public partial class TDKAnalyticsService : TDKBaseService
    {
        private string cacheFolderPath;
        private readonly object fileLock = new object();

        private List<Dictionary<string, object>> eventBuffer = new List<Dictionary<string, object>>();
        private const int MaxBufferSize = 20; // Maximum number of events in the buffer before writing to file

        private void SetupPaths()
        {
            this.cacheFolderPath = Path.Combine(Application.persistentDataPath, AnalyticsConstants.CACHE_DIRECTORY_NAME);
            
            if (!Directory.Exists(cacheFolderPath))
            {
                Directory.CreateDirectory(cacheFolderPath);
            }

#if UNITY_IOS   
            Device.SetNoBackupFlag(cacheFolderPath);
#endif
        }

        private async void CacheNewEvent(string eventName, IDictionary<string, object> parameters = null)
        {
            // construct log entry as a JSON object
            var logEntry = new Dictionary<string, object>
            {
                { "event_name", eventName },
                { "parameters", parameters ?? new Dictionary<string, object>() }
            };

            lock (eventBuffer)
            {
                eventBuffer.Add(logEntry);

                // check if buffer size exceeds the maximum threshold
                if (eventBuffer.Count >= MaxBufferSize)
                {
                    // flush buffer and write events to file
                    FlushBufferToFile();
                }
            }
        }

        private async Task FlushBufferToFile()
        {
            // create a log file with the current date
            string logFilePath = Path.Combine(cacheFolderPath, $"{DateTime.Now:yyyy-MM-dd}.log");

            // serialize events in the buffer to json
            string jsonEvents = JsonConvert.SerializeObject(eventBuffer);

            try
            {
                // asynchronously write events to file
                await Task.Run(() =>
                {
                    lock (fileLock)
                    {
                        // append events to the log file
                        File.AppendAllText(logFilePath, jsonEvents + Environment.NewLine);
                    }
                });
            }
            catch (Exception ex)
            {
                // handle exceptions (e.g., logging or displaying an error)
                TDKLogger.LogError($"[TDKAnalyticsService.PersistPayloadToDiskAsync] Error logging events: {ex.Message}");
            }
            finally
            {
                // clear the buffer after writing to file
                lock (eventBuffer)
                {
                    eventBuffer.Clear();
                }
            }
        }

        private async void PersistPayloadToDiskAsync(string payload)
        {
            await Task.Run(() =>
            {
                // simulate writing the payload to disk
                string filePath = Path.Combine(cacheFolderPath, "analytics_payload.txt");
                File.WriteAllText(filePath, payload);
                TDKLogger.Log("[TDKAnalyticsService.PersistPayloadToDiskAsync] Payload persisted to disk: " + filePath);
            });
        }
    }
}
