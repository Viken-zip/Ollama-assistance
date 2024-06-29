using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing;
using System.Diagnostics;
using WinForms = System.Windows.Forms;
using Ollama_assistance.ViewModel;
using System.Collections.ObjectModel;
using Ollama_assistance.Services;
using Ollama_assistance.Views;
using System.Threading;
using Microsoft.VisualBasic.Devices;
using Keyboard = System.Windows.Input.Keyboard;

namespace Ollama_assistance
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainViewModel _viewModel;
        private ConfigViewModel _configViewModel;
        public ObservableCollection<string> ChatMessages { get; set; }
        
        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new MainViewModel();
            this.DataContext = _viewModel;
            ChatMessages = new ObservableCollection<string>(ChatHistoryService.LoadChatHistory());
            this.Loaded += MainWindow_Loaded;

            _configViewModel = new ConfigViewModel();
            var config = _configViewModel.GetConfig();
            
            if (config.ShowSystemUsage) //this should be fine as long show SystemUsage isn't used with OnPropertyChanged
            {
                Thread SystemUsageThread = new Thread(SystemUsage);
                SystemUsageThread.IsBackground = true;
                SystemUsageThread.Start();
            }
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            Window window = (Window)sender;
            window.Topmost = true;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {

            for (int i = 0; i < ChatMessages.Count; i++)
            {
                string message = clearSenderOfMessage(ChatMessages[i]);
                RenderMessage(
                    message, 
                    ChatMessages[i].Contains("User:") ? "User" : "AI" 
                    );
            }
            chatScrollViewer.ScrollToBottom();
            PythonIntegration.StartServer();

            MicIsOn(GlobalStateService.Instance.IsMicrophoneOn);
            this.KeyUp += new KeyEventHandler(MicKey_KeyUp);
        }

        private void MicKey_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.M && Keyboard.Modifiers == ModifierKeys.Control)
            {
                GlobalStateService.Instance.IsMicrophoneOn = !GlobalStateService.Instance.IsMicrophoneOn;
                MicIsOn(GlobalStateService.Instance.IsMicrophoneOn);
                PythonIntegration.CheckMicrophoneSTT();
            }
        }
        
        public void MicIsOn(bool isMicOn)
        {
            MicStatus.Foreground = new SolidColorBrush( isMicOn ? Colors.LightGreen : Colors.Red );  
        }

        private string clearSenderOfMessage(string message)
        {
            //this isn't necessary but just in case.
            if (message.Contains(": ")) {
                return message.Substring(message.IndexOf(": ") + 2);
            } else { 
                return message; 
            }
        }

        private void SetWindowPosition(int screenIndex)
        {
            WinForms.Screen[] screens = WinForms.Screen.AllScreens;

            if (screenIndex < 0 || screenIndex >= screens.Length)
            {
                System.Windows.MessageBox.Show("Screen index out of range.");
                return;
            }

            WinForms.Screen targetScreen = screens[screenIndex];
            System.Drawing.Rectangle workingArea = targetScreen.WorkingArea;

            this.WindowStartupLocation = WindowStartupLocation.Manual;
            this.Width = workingArea.Width / 5;
            this.Height = workingArea.Height / 2;
            this.Left = workingArea.Left + (workingArea.Width - (this.Width + 25));
            this.Top = workingArea.Top + (workingArea.Height - (this.Height + 25));
        }

        private void messageInputBox_keyDown (object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                SendMessage();
            }
        }

        private void sendMessage_click(object sender, RoutedEventArgs e)
        {
            SendMessage();
        }

        public void AddMessageToHistory(string message, string sender)
        {
            _viewModel.SendMessage($"{sender}: " + message);
        }

        private async void SendMessage()
        {
            string message = messageInputBox.Text;
            if (!string.IsNullOrEmpty(message) )
            {
                RenderMessage(message, "User");

                messageInputBox.Clear();
                chatScrollViewer.ScrollToBottom();

                _viewModel.SendMessage("User: " + message);


                PythonIntegration.AskAI(message);
            }
        }

        public void RenderMessage(string message, string sender)
        {
            if (!string.IsNullOrEmpty(message))
            {
                TextBlock textBlock = new TextBlock
                {
                    Text = message,
                    Background = new SolidColorBrush( (sender == "User") ? Colors.White : Colors.LightBlue),
                    TextWrapping = TextWrapping.Wrap
                };

                Border border = new Border
                {
                    CornerRadius = new CornerRadius(8),
                    BorderThickness = new Thickness(3),
                    BorderBrush = new SolidColorBrush( (sender == "User") ? Colors.White : Colors.LightBlue ),
                    Margin = ( (sender == "User") ? new Thickness(200, 5, 5, 5) : new Thickness(5, 5, 200, 5) ),
                    Child = textBlock
                };

                chatContainer.Children.Add(border);
            }
        }

        private void configBtnClick(object sender, RoutedEventArgs e)
        {
            ConfigWindow configWindow = new ConfigWindow();
            configWindow.Show();
        }

        private void MinimizeButtonClick(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void CloseButtonClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void SystemUsage()
        {
            PerformanceCounter cpuCounter;
            PerformanceCounter ramCounter;

            cpuCounter = new PerformanceCounter("processor Information", "% Processor Utility", "_Total");
            ramCounter = new PerformanceCounter("Memory", "Available MBytes");
            
            float totalRamAmount = new ComputerInfo().TotalPhysicalMemory;
            
            while (true)
            {
                float cpuUsage = cpuCounter.NextValue();
                float ramUsage = Math.Abs((ramCounter.NextValue() / 1024) - (totalRamAmount / 1024 / 1024 / 1024));
                
                Application.Current.Dispatcher.Invoke(() =>
                {
                    CpuUsage.Text = $"CPU: {cpuUsage.ToString("0.0")}%";
                    RamUsage.Text = $"RAM: {ramUsage.ToString("0.0")}GB";
                });
                Thread.Sleep(1000);
            }
        }
    }
}
