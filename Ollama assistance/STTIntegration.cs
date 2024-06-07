using Ollama_assistance.Services;
using Python.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Ollama_assistance
{
    internal class STTIntegration
    {
        private static ConfigService configService;
        private static string PythonDLLPath;
        private static string PythonDLLsPath;

        private static string fifoPath = "STT_fifo"; //or name whatever

        static STTIntegration()
        {
            configService = new ConfigService();
            Config config = configService.getConfig();

            PythonDLLPath = config.PyDLLPath;
            PythonDLLsPath = config.PyDLLsPath;
        }

        private static string STTScriptName = "STTScript"; //STTPipeServer
        private static string PathToSTTScript = Path.Combine(Directory.GetCurrentDirectory(), $"{STTScriptName}.py");

        
        static CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private static Task pythonTask;
        public static void StartSTTPythonServer()
        {
            pythonTask = Task.Run(() => StartSTTPython(), cancellationTokenSource.Token);
            var readTask = ReadFromPipeAsync(fifoPath);
        }

        private static void StartSTTPython()
        {
            Runtime.PythonDLL = PythonDLLPath;
            PythonEngine.Initialize();
            try
            {
                using (Py.GIL())
                {
                    PythonEngine.Exec($"import sys\nsys.path.append(r'{PythonDLLsPath}')");
                    using (var scope = Py.CreateScope())
                    {
                        var scriptFile = $"{STTScriptName}.py";
                        var scriptContents = File.ReadAllText(scriptFile);

                        var compiledCode = PythonEngine.Compile(scriptContents);
                        scope.Execute(compiledCode);

                        dynamic runServer = scope.Get("run_server");
                        runServer();
                    }
                }
            }
            catch (PythonException ex)
            {
                MessageBox.Show(ex.ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        public static async void StopSTTPythonServer()
        {
            string shutDownFlag = "shutdown.flag";
            File.Create(shutDownFlag).Dispose();

            cancellationTokenSource.Cancel();
            if(pythonTask != null)
            {
                await pythonTask;
            }

            PythonEngine.Shutdown();
        }

        static async Task ReadFromPipeAsync(string fifoPath)
        {
            while (!cancellationTokenSource.Token.IsCancellationRequested)
            {
                using (var pipeClient = new NamedPipeClientStream(".", fifoPath, PipeDirection.In))
                {
                    try
                    {
                        pipeClient.Connect(1000); //time out in millisecond might need to make it higer
                        using (var sr = new StreamReader(pipeClient))
                        {
                            string STTResult = await sr.ReadToEndAsync();
                            if (!string.IsNullOrEmpty(STTResult))
                            {
                                MessageBox.Show(STTResult);
                            }
                        }
                    }
                    catch (TimeoutException) 
                    { 
                        //let it retry until it crashes because it will ;)
                    }
                }
                await Task.Delay(1000);
            }
        }
    }
}
