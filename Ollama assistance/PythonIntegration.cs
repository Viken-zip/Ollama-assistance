using Ollama_assistance.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Python.Runtime;
using System.Windows;
using System.Threading;
using System.IO.Pipes;

namespace Ollama_assistance
{
    class PythonIntegration
    {

        private static ConfigService configService;
        private static string PythonDLLPath;
        private static string PythonDLLsPath;
        private static string ScriptName = "PipeServer";
        private static string PathToScript = Path.Combine(Directory.GetCurrentDirectory(), $"{ScriptName}.py");

        static CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private static Task PythonTask;

        // paths might also be called name in this case but maybe change in future
        //pipe to get from python to C#
        private static string STTLoadedPipePath = "STTLoaded_pipe";
        private static string STTResultPipePath = "STTResult_pipe";
        private static string AIAnswerPipePath = "AIAnswer_pipe";

        //pipes to send from C# to python
        private static string AIQuestionPipePath = "AIQuestion_pipe";
        private static string STTOnPipePath = "STTOn_pipe";

        static PythonIntegration()
        {
            configService = new ConfigService();
            Config config = configService.getConfig();

            PythonDLLPath = config.PyDLLPath;
            PythonDLLsPath = config.PyDLLsPath;
        }

        public static void StartServer()
        {
            PythonTask = Task.Run(() => StartPythonServer(), cancellationTokenSource.Token);
            //var readTask = ReadFromPipeAsync(fifoPath);

            /*var readTasks = new List<Task>
            {
                ReadFromPipeAsync(STTLoadedPipePath, "STTLoaded"),
                ReadFromPipeAsync(STTResultPipePath, "STTResult"),
                ReadFromPipeAsync(AIAnswerPipePath, "AIAnswer")
            };
            Task.WhenAll(readTasks);*/

            Task.Run(() => ReadFromPipeAsync(STTLoadedPipePath, "STTLoaded"), cancellationTokenSource.Token);
            Task.Run(() => ReadFromPipeAsync(STTResultPipePath, "STTResult"), cancellationTokenSource.Token);
            Task.Run(() => ReadFromPipeAsync(AIAnswerPipePath, "AIAnswer"), cancellationTokenSource.Token);
        }

        private static void StartPythonServer()
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
                        var scriptFile = $"{ScriptName}.py";
                        var scriptContent = File.ReadAllText(scriptFile);

                        var compileCode = PythonEngine.Compile(scriptContent);
                        scope.Execute(compileCode);

                        dynamic startServer = scope.Get("start_server");
                        startServer();
                    }
                }
            }
            catch (PythonException ex)
            {
                MessageBox.Show(ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public static async void StopServer()
        {
            string shutDownFlag = "shutdown.flag";
            File.Create(shutDownFlag).Dispose();

            cancellationTokenSource.Cancel();
            if(PythonTask != null)
            {
                await PythonTask;
            }

            PythonEngine.Shutdown();
        }

        //pipe hell :|
        static async Task ReadFromPipeAsync(string pipePath, string pipeId)
        {
            var  actions = new Dictionary<string, Action<string>> 
            {
                { "STTLoaded", HandleSTTLoaded },
                { "STTResult", HandleSTTResult },
                { "AIAnswer", HandleAIAnswer }
            };

            while (!cancellationTokenSource.Token.IsCancellationRequested)
            {
                using(var pipeClient = new NamedPipeClientStream(".", pipePath, PipeDirection.In))
                {
                    try
                    {
                        pipeClient.Connect(1000);
                        using (var sr = new StreamReader(pipeClient))
                        {
                            string result = await sr.ReadToEndAsync();
                            if (!string.IsNullOrEmpty(result))
                            {
                                if (actions.ContainsKey(pipeId))
                                {
                                    actions[pipeId](result);
                                }
                            }
                        }
                    }
                    catch (TimeoutException)
                    {
                        //retry till crash or shutdown
                    }
                }
                await Task.Delay(1000);
            }
        }

        static void createPipe(string pipeName, string message) // or sendPipe whatterver makes pipe for python to pickup
        {
            using (NamedPipeServerStream pipeServer = new NamedPipeServerStream(pipeName, PipeDirection.Out))
            {
                pipeServer.WaitForConnection();
                using (StreamWriter write = new StreamWriter(pipeServer))
                {
                    write.WriteLine(message);
                }
            }
        }

        //makes pipes for python to receive
        public static void AskAI(string question)
        {
            createPipe(AIQuestionPipePath, question);
        }

        // run when gest message from pipe
        static void HandleSTTLoaded(string result)
        {

        }

        static void HandleSTTResult(string result)
        {

        }

        static void HandleAIAnswer(string result)
        {
            MessageBox.Show(result);
        }
    }
}
