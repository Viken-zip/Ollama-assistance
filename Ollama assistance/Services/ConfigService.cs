using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Ollama_assistance.Services
{
    public class Config
    {
        [JsonPropertyName("PyDLLPath")]
        public string PyDLLPath { get; set; }

        [JsonPropertyName("PyDLLsPath")]
        public string PyDLLsPath { get; set;}
        
        [JsonPropertyName("ShowSystemUsage")]
        public bool ShowSystemUsage { get; set; }
    }

    public class ConfigService
    {
        private static string GetAppDataDirectory()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string appDirectory = Path.Combine(appDataPath, "OllamaAssistance");
            if (!Directory.Exists(appDirectory))
            {
                Directory.CreateDirectory(appDirectory);
            }
            return appDirectory;
        }

        private static string GetConfigFilePath()
        {
            string appDirectory = GetAppDataDirectory();
            return Path.Combine(appDirectory, "Config.json");
        }

        private void NewConfigFile()
        {
            Config newConfig = new Config
            {
                PyDLLPath = "",
                PyDLLsPath = "",
                ShowSystemUsage = false
            };
            string jsonString = JsonSerializer.Serialize(newConfig, new JsonSerializerOptions { WriteIndented = true });
            string filePath = GetConfigFilePath();
            File.WriteAllText(filePath, jsonString);
        }

        private Config LoadJson()
        {
            string jsonFilePath = GetConfigFilePath();
            if (!File.Exists(jsonFilePath))
            {
                NewConfigFile();
            }

            string jsonString = File.ReadAllText(jsonFilePath);
            Config config = JsonSerializer.Deserialize<Config>(jsonString);

            return config;
        }

        private void SaveJson(Config newConfig)
        {
            string jsonString = JsonSerializer.Serialize(newConfig, new JsonSerializerOptions { WriteIndented = true });
            string filePath = GetConfigFilePath();
            File.WriteAllText(filePath, jsonString);
        }

        public void UpdateConfig(Config newConfig)
        {
            SaveJson(newConfig); //dose this work?
        }

        public Config getConfig()
        {
            Config activeConfig = LoadJson();
            return activeConfig;
        }
    }
}
