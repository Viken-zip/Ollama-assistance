using Python.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Ollama_assistance
{
    internal class STTIntegration
    {

        private static string PythonDLLPath = @"";
        private static string PythonDLLsPath = @"";

        private static string OllamaScriptName = "STTScript";
        private static string PathToOllamaScript = Path.Combine(Directory.GetCurrentDirectory(), $"{OllamaScriptName}.py");

        public static async Task<string> STT()
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
                            var scriptFile = "STTScript.py";
                            var scriptContents = File.ReadAllText(scriptFile);

                            var compiledCode = PythonEngine.Compile(scriptContents, scriptFile);
                            scope.Execute(compiledCode);

                            dynamic? STTFunction = scope.Get("main");
                            result = STTFunction();
                        }
                    }
                }
                catch (PythonException ex)
                {
                    MessageBox.Show($"Python error: {ex.ToString()}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Python error: {ex.ToString()}");
                }
                finally
                {
                    PythonEngine.Shutdown();
                }

                return result;
            });
        }
    }
}
