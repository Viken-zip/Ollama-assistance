using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Ollama_assistance.Services
{
    class ChatHistoryService
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

        private static string GetChatHistoryFilePath()
        {
            string appDirectory = GetAppDataDirectory();
            return Path.Combine(appDirectory, "ChatHistoryService.txt");
        }

        public static List<string> LoadChatHistory()
        {
            string filePath = GetChatHistoryFilePath();
            if (File.Exists(filePath))
            {
                return new List<string>(File.ReadAllLines(filePath));
            }
            return new List<string>();
        }

        public static void SaveMessageToHistory(string message)
        {
            string filePath = GetChatHistoryFilePath();
            File.AppendAllText(filePath, message + Environment.NewLine);
        }
    }
}
