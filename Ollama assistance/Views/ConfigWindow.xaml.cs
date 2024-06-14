using Ollama_assistance.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Ollama_assistance.ViewModel;
using MessageBox = System.Windows.MessageBox;

namespace Ollama_assistance.Views
{
    /// <summary>
    /// Interaction logic for ConfigWindow.xaml
    /// </summary>
    public partial class ConfigWindow : Window
    {
        private ConfigViewModel _configViewModel;
        
        private static ConfigService configService;
        private Config config;
        public ConfigWindow()
        {
            configService = new ConfigService();
            config = configService.getConfig();
            InitializeComponent();

            _configViewModel = new ConfigViewModel();
            this.DataContext = _configViewModel;
            
            this.Loaded += Window_Loaded;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //PythonDLLPath.Text = config.PyDLLPath;
            //PythonDLLsPath.Text = config.PyDLLsPath;
            //ShowSystemUsageToggle.IsChecked = config.ShowSystemUsage;
        }

        private void saveBtnClick(object sender, RoutedEventArgs e) 
        { 
            config.PyDLLPath = PythonDLLPath.Text.Replace("\"", ""); // ill just keep the replace there just in case, might remove soon
            config.PyDLLsPath = PythonDLLsPath.Text.Replace("\"", "");

            //MessageBox.Show( (ShowSystemUsageToggle.IsChecked == true ? true : false).ToString() );
            
            //config.ShowSystemUsage = (ShowSystemUsageToggle.IsChecked == true ? true : false);

            
            
            configService.UpdateConfig(config);
            this.Close();
        }

        private void cancelBtnClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void removeUnwantedSymbols(object sender, RoutedEventArgs e)
        {
            var textBox = sender as System.Windows.Forms.TextBox;
            if (textBox != null)
            {
                string Text = textBox.Text;
                string newText = Text.Replace("\"", "");

                if (Text != newText)
                {
                    textBox.Text = newText;
                }
            }
        }

        
        
    }
}
