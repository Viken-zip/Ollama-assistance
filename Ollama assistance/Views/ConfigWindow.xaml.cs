using Ollama_assistance.Services;
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
using System.Windows.Shapes;

namespace Ollama_assistance.Views
{
    /// <summary>
    /// Interaction logic for ConfigWindow.xaml
    /// </summary>
    public partial class ConfigWindow : Window
    {
        private static ConfigService configService;
        private Config config;
        public ConfigWindow()
        {
            configService = new ConfigService();
            config = configService.getConfig();
            InitializeComponent();
            this.Loaded += Window_Loaded;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            PythonDLLPath.Text = config.PyDLLPath;
            PythonDLLsPath.Text = config.PyDLLsPath;
        }

        private void saveBtnClick(object sender, RoutedEventArgs e) 
        { 
            config.PyDLLPath = PythonDLLPath.Text.Replace("\"", ""); // ill just keep the replace there just in case, might remove soon
            config.PyDLLsPath = PythonDLLsPath.Text;
            configService.UpdateConfig(config);
        }

        private void cancelBtnClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void removeUnwantedSymbols(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
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
