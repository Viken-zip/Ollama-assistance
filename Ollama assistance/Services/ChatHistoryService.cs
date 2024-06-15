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
            if (!File.Exists(filePath))
            {
                return new List<string>();
            }

            var lines = File.ReadAllLines(filePath);
            var messages = new List<string>();
            var currentMessage = new StringBuilder();
            string currentSender = null;

            foreach (var line in lines)
            {
                if (line.StartsWith("User: ") || line.StartsWith("AI: "))
                {
                    if (currentMessage.Length > 0)
                    {
                        messages.Add(currentMessage.ToString()); // do not Trim
                        currentMessage.Clear();
                    }

                    currentSender = line.StartsWith("User: ") ? "User: " : "AI: ";
                    currentMessage.Append(line);
                }                
                else if (currentSender != null)
                {
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        currentMessage.AppendLine();
                    }
                    else
                    {
                        currentMessage.AppendLine(line);
                    }
                    
                }
            }

            if(currentMessage.Length > 0)
            {
                messages.Add(currentMessage.ToString()); // do not Trim
            }

            return messages;
        }

        public static void SaveMessageToHistory(string message)
        {
            string filePath = GetChatHistoryFilePath();
            File.AppendAllText(filePath, message + Environment.NewLine);
        }
    }
}
