using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Treasure
{
    public class LocalSettings
    {
        public Dictionary<string, object> Settings { get; set; }
        
        private static readonly string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MyGameSettings.json");

        public void Save()
        {
            string json = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }

        public static LocalSettings Load()
        {
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                return JsonConvert.DeserializeObject<LocalSettings>(json);
            }
            return new LocalSettings();
        }
    }
}
