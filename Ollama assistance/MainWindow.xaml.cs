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
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
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
