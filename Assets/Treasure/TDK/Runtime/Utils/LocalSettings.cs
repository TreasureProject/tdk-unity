using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Treasure
{
    public class LocalSettings
    {
        public Dictionary<string, object> Settings { get; set; }
        
        private static string _filePath;

        public LocalSettings(string settingsDirectory)
        {
            _filePath = Path.Combine(settingsDirectory, "local_settings.json");

            if (!File.Exists(_filePath))
            {
                Settings = new Dictionary<string, object>();
                Save();
            }

            string json = File.ReadAllText(_filePath);
            Settings = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
        }

        public void Save()
        {
            string json = JsonConvert.SerializeObject(Settings, Formatting.Indented);
            File.WriteAllText(_filePath, json);
        }

        public T Get<T>(string key)
        {
            if (Settings.ContainsKey(key))
            {
                return (T)Settings[key];
            }
            return default(T);
        }
        
        public void Set<T>(string key, T value)
        {
            Settings[key] = value;
            Save();
        }

        public void Delete(string key)
        {
            bool didRemove = Settings.Remove(key);
            if (didRemove)
            {
                Save();
            }
        }
    }
}
