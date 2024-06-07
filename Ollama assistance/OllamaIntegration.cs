using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Ollama_assistance.Services;
using Python.Runtime;

namespace Ollama_assistance
{
    class OllamaIntegration
    {


        // will make it easier to add path and save path in a config file soon
        private static ConfigService configService;// = new ConfigService();
        //private static Config config = configService.getConfig();
        private static string PythonDLLPath; // = config.PyDLLPath;
        private static string PythonDLLsPath; // = config.PyDLLsPath;

        
        

        static OllamaIntegration()
        {
            configService = new ConfigService();
            Config config = configService.getConfig(); //doesn't take updated info from config, needs fixing soon.

            PythonDLLPath = config.PyDLLPath;
            PythonDLLsPath = config.PyDLLsPath;
        }

        private static string OllamaScriptName = "Ollama";
        private static string PathToOllamaScript = Path.Combine(Directory.GetCurrentDirectory(), $"{OllamaScriptName}.py"); // to be added python script for using Ollama
        public static async Task<string> AskOllama(string question)
        {
            return await Task.Run(() => {
                Runtime.PythonDLL = PythonDLLPath;
                PythonEngine.Initialize();

                string? result = null;

                try
                {
                    using (Py.GIL())
                    {
                        PythonEngine.Exec($"import sys\nsys.path.append(r'{PythonDLLsPath}')");

                        using (var scope = Py.CreateScope())
                        {
                            var scriptFile = "Ollama.py";
                            var scriptContents = File.ReadAllText(scriptFile);

                            var compiledCode = PythonEngine.Compile(scriptContents, scriptFile);
                            scope.Execute(compiledCode);

                            dynamic? AskOllamaFunction = scope.Get("AskOllama");
                            result = AskOllamaFunction(question);

                        }
                    }
                } catch (PythonException ex)
                {
                    MessageBox.Show($"Python error: {ex.ToString()}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Python error: {ex.ToString()}");
                }
                finally
                {
                    PythonEngine.Shutdown(); // this might make it slow, would be a better idea to always keep the script active i suppose.
                }

                //return "test answer"; //temporary for testin purposes
                return result;
            });
        }
    }
}
