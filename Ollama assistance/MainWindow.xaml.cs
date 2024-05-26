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
using WinForms = System.Windows.Forms;
using Ollama_assistance.ViewModel;

namespace Ollama_assistance
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainViewModel _viewModel;
        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new MainViewModel();
            DataContext = _viewModel;
            this.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //SetWindowPosition(0);
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

            //MessageBox.Show($"{this.Width}x{this.Height}");
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

        private void SendMessage()
        {
            string message = messageInputBox.Text;
            if (!string.IsNullOrEmpty(message) )
            {
                TextBlock textBlock = new TextBlock
                {
                    Text = message,
                    Background = new SolidColorBrush(Colors.White)
                };

                Border border = new Border
                {
                    CornerRadius = new CornerRadius(8),
                    BorderThickness = new Thickness(3),
                    BorderBrush = new SolidColorBrush(Colors.White),
                    Margin = new Thickness(200,5,5,5),
                    Child = textBlock
                };

                chatContainer.Children.Add(border);
                messageInputBox.Clear();
                chatScrollViewer.ScrollToBottom();

                _viewModel.SendMessage(message);
            }
        }

        private void MinimizeButtonClick(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void CloseButtonClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
