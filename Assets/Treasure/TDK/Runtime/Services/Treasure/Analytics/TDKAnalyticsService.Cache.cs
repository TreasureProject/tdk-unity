#if !UNITY_WEBGL
using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace Treasure
{
    public partial class TDKAnalyticsService : TDKBaseService
    {
        private List<string> _memoryCache;
        private string _diskCachePath;

        private Timer _flushCacheTimer;

        private async void InitEventCaching()
        {
            // Initialize memory cache
            _memoryCache = new List<string>();
            
            // Memory cache flushing
            StartPeriodicMemoryFlush();

            // Setup disk cache
            _diskCachePath = Path.Combine(
                TDK.Instance.AbstractedEngineApi.ApplicationPersistentDataPath(),
                AnalyticsConstants.PERSISTENT_DIRECTORY_NAME
            );
            
            if (!Directory.Exists(_diskCachePath))
            {
                Directory.CreateDirectory(_diskCachePath);
            }
            TDKLogger.Log("[TDKAnalyticsService.Cache:InitPersistentCache] _persistentFolderPath: " + _diskCachePath);

            // Flush disk cache on startup
            await FlushDiskCache();
        }

        private void StartPeriodicMemoryFlush()
        {
            _flushCacheTimer = new Timer(async _ => {
                try {
                    await FlushMemoryCache();
                }
                catch (Exception e) {
                    TDKLogger.Log("[TDKAnalyticsService.Cache:PeriodicMemoryFlush] uncaught error flushing cache: " + e.Message);
                }
                
            });
            ResetFlushTimer();
        }

        private async void TerminateCacheFlushing()
        {
            _flushCacheTimer?.Dispose();
            _flushCacheTimer = null;
            await FlushMemoryCache();
        }

        private void ResetFlushTimer()
        {
            _flushCacheTimer?.Change(
                TimeSpan.FromSeconds(AnalyticsConstants.CACHE_FLUSH_TIME_SECONDS),
                TimeSpan.FromSeconds(AnalyticsConstants.CACHE_FLUSH_TIME_SECONDS)
            );
		}

        private async Task FlushMemoryCache()
        {
            List<string> memoryCacheCopy;
            lock (_memoryCache)
            {
                memoryCacheCopy = new List<string>(_memoryCache);
                _memoryCache.Clear();
            }

            if(memoryCacheCopy.Count > 0)
            {
                // Send the batch of events for io
                var success = await SendEventBatch(memoryCacheCopy);

                // If the request failed, persist the payload to disk in a separate task
                if(!success)
                {
                    PersistEventBatch(memoryCacheCopy);
                }
            }

            await FlushDiskCache();
        }

        private Task FlushDiskCache()
        {
            return Task.Run(async () =>
            {
                // DeleteOldBatches();

                string[] files = Directory.GetFiles(_diskCachePath, "*.eventbatch");
                if(files.Length > 0)
                {
                    TDKLogger.Log("[TDKAnalyticsService.Cache:FlushDiskCache] processing " + files.Length + " persisted batches");
                }

                foreach (string filePath in files)
                {
                    string content = File.ReadAllText(filePath);
                    string fileName = Path.GetFileName(filePath);
                    string localSettingsKey = fileName + "_sendattemps";

                    int numSendAttempts = 0;
                    try {
                        numSendAttempts = TDK.Instance.LocalSettings.Get<int>(localSettingsKey);
                    }
                    catch(Exception e) {
                        TDKLogger.Log("[TDKAnalyticsService.Cache:FlushDiskCache] local settings key not found: " + e.Message);
                        // set key to 0 in case the error is due to the value not being a valid int
                        TDK.Instance.LocalSettings.Set<int>(localSettingsKey, 0);
                    }

                    // If max send attempts have been reached, delete the file and stop processing
                    if(numSendAttempts > AnalyticsConstants.PERSISTENT_MAX_RETRIES) {
                        File.Delete(filePath);
                        TDK.Instance.LocalSettings.Delete(localSettingsKey);
                        break;
                    }

                    // Send the disk events for io
                    var success = await SendEventBatch(content);

                    if(success) {
                        // Delete the file if the send was successful
                        File.Delete(filePath);
                        
                        // Clear the localSettings key
                        try {
                            TDK.Instance.LocalSettings.Delete(localSettingsKey);
                        }
                        catch(Exception e) {
                            TDKLogger.Log("[TDKAnalyticsService.Cache:FlushDiskCache] local settings key removal failed: " + e.Message);
                        }   
                        TDKLogger.Log("[TDKAnalyticsService.Cache:FlushDiskCache] removing cache file: " + filePath);
                    }
                    else {
                        // Increment the localSettings send attempt if send failed
                        TDK.Instance.LocalSettings.Set<int>(localSettingsKey, numSendAttempts + 1);
                    }
                }
            });
        }
        /// <dev>
        /// Delete old batches from disk cache that are older than 30 days
        /// </dev>
        private void DeleteOldBatches()
        {
            try
            {
                var directoryInfo = new DirectoryInfo(_diskCachePath);

                var files = directoryInfo.GetFiles()
                    .Where(file => file.LastWriteTime < DateTime.Now.AddDays(-30))
                    .ToList();

                foreach (var file in files)
                {
                    file.Delete();
                }
            }
            catch (Exception ex)
            {
                TDKLogger.Log("[TDKAnalyticsService.Cache:DeleteOldBatches] Error deleting old batch: " + ex.Message);
            }
        }

        private async void CacheEvent(string evtJsonStr)
        {
            if(_memoryCache.Count + 1 > AnalyticsConstants.MAX_CACHE_EVENT_COUNT ||
                CalculateMemoryCacheSizeInBytes() + evtJsonStr.Length > AnalyticsConstants.MAX_CACHE_SIZE_KB * 1024)
            {
                // Flush the cache if limits are exceeded
                TDKLogger.Log("[TDKAnalyticsService.Cache:CacheEvent] Cache size exceeded, flushing cache");
                ResetFlushTimer(); // set flush timer back to 0 since we are triggering the flush manually
                await FlushMemoryCache();
            }

            lock (_memoryCache)
            {
                _memoryCache.Add(evtJsonStr);
            }
        }

        private int CalculateMemoryCacheSizeInBytes()
        {
            // calculate the total size of the cached events in bytes
            return _memoryCache.Sum(e => e.Length);
        }

        private async void PersistEventBatch(List<string> events)
        {
            await Task.Run(() =>
            {
                string payload = "[" + string.Join(",", events) + "]";

                // write the payload to disk
                var fileGuid = Guid.NewGuid().ToString("N");
                string filePath = Path.Combine(_diskCachePath, $"tdk_{fileGuid}.eventbatch");
                File.WriteAllText(filePath, payload);
                TDKLogger.Log("[TDKAnalyticsService.Cache:PersistPayloadToDiskAsync] Payload persisted to disk: " + filePath);
            });
        }
    }
}
#endif
