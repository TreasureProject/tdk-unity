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

        // Memory cache flushing
        private Thread _flushThread;
        private bool _flushThreadIsRunning;

        private void InitEventCaching()
        {
            // Initialize memory cache
            _memoryCache = new List<string>();

            // Memory cache flushing
            _flushThreadIsRunning = true;
            _flushThread = new Thread(StartPeriodicMemoryFlush);
            _flushThread.Start();

            // Setup disk cache
            _diskCachePath = Path.Combine(TDK.Instance.PersistentDataPath, AnalyticsConstants.PERSISTENT_DIRECTORY_NAME);
            
            if (!Directory.Exists(_diskCachePath))
            {
                Directory.CreateDirectory(_diskCachePath);
            }
            TDKLogger.Log("[TDKAnalyticsService.Cache:InitPersistentCache] _persistentFolderPath: " + _diskCachePath);
        }

        private void StartPeriodicMemoryFlush()
        {
            while (_flushThreadIsRunning)
            {
                Thread.Sleep(AnalyticsConstants.CACHE_FLUSH_TIME_SECONDS * 1000);
                FlushMemoryCache().Wait();
            }
        }

        private async Task StopPeriodicMemoryFlush()
        {
            _flushThreadIsRunning = false;
            
            // Wait for the flushing thread to finish
            _flushThread.Join();

            // Ensure any remaining cache data is flushed to disk
            await FlushMemoryCache();
        }

        private async void TermintateCacheMemoryFlush()
        {
            await StopPeriodicMemoryFlush();
            _flushThread.Abort();
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
                var localSettings = LocalSettings.Load();

                string[] files = Directory.GetFiles(_diskCachePath, "*.eventbatch");

                foreach (string filePath in files)
                {
                    TDKLogger.Log("[TDKAnalyticsService.Cache:FlushDiskCache] processing: " + filePath);
                    string content = File.ReadAllText(filePath);
                    string fileName = Path.GetFileName(filePath);
                    string localSettingsKey = fileName + "_sendattemps";

                    int numSendAttempts = 0;
                    try {
                        numSendAttempts = (int) localSettings.Settings[localSettingsKey];
                    }
                    catch(Exception e) {
                        TDKLogger.Log("[TDKAnalyticsService.Cache:FlushDiskCache] local settings key not found: " + e.Message);
                    }

                    // If max send attempts have been reached, delete the file and stop processing
                    if(numSendAttempts > AnalyticsConstants.PERSISTENT_MAX_RETRIES) {
                        File.Delete(filePath);
                        break;
                    }

                    // Send the disk events for io
                    var success = await SendEventBatch(content);

                    if(success) {
                        // Delete the file if the send was successful
                        File.Delete(filePath);
                        
                        // Clear the localSettings key
                        try {
                            localSettings.Settings.Remove(localSettingsKey);
                            localSettings.Save();
                        }
                        catch(Exception e) {
                            TDKLogger.Log("[TDKAnalyticsService.Cache:FlushDiskCache] local settings key removal failed: " + e.Message);
                        }   
                        TDKLogger.Log("[TDKAnalyticsService.Cache:FlushDiskCache] removing cache file: " + filePath);
                    }
                    else {
                        // Increment the localSettings send attempt if send failed
                        localSettings.Settings[localSettingsKey] = numSendAttempts + 1;
                        localSettings.Save();
                    }
                }
            });
        }

        public async void CacheEvent(string evtJsonStr)
        {
            TDKLogger.Log("[TDKAnalyticsService.Cache:CacheEvent] caching event: " + evtJsonStr);

            if(_memoryCache.Count + 1 > AnalyticsConstants.MAX_CACHE_EVENT_COUNT ||
                CalculateMemoryCacheSizeInBytes() + evtJsonStr.Length > AnalyticsConstants.MAX_CACHE_SIZE_KB * 1024)
            {
                // Flush the cache if limits are exceeded
                TDKLogger.Log("[TDKAnalyticsService.Cache:CacheEvent] Cache size exceeded, flushing cache");
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
