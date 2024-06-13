using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using Ollama_assistance.Services;
using Ollama_assistance.Utils;

namespace Ollama_assistance.ViewModel
{
    class ConfigViewModel : INotifyPropertyChanged
    {
        private ConfigService _configService;
        private Config _config;
        
        public PythonDLLPath PythonDLLPath { get; set; }
        public PythonDLLsPath PythonDLLsPath { get; set; }
        public ShowSystemUsage ShowSystemUsage { get; set; }
        
        public ICommand UpdatePythonDLLPathCommand { get; set; }
        public ICommand UpdatePythonDLLsPathCommand { get; set; }
        public ICommand UpdateShowSystemUsageCommand { get; set; }
        
        public ICommand SaveCommand { get; }
        
        public ConfigViewModel()
        {
            _configService = new ConfigService();
            _config = _configService.getConfig();

            PythonDLLPath = new PythonDLLPath { Path = _config.PyDLLPath };
            PythonDLLsPath = new PythonDLLsPath { Path = _config.PyDLLsPath};
            ShowSystemUsage = new ShowSystemUsage { Show = _config.ShowSystemUsage };
            
            UpdatePythonDLLPathCommand = new RelayCommand(UpdatePythonDLLPath);
            UpdatePythonDLLsPathCommand = new RelayCommand(UpdatePythonDLLsPath);
            UpdateShowSystemUsageCommand = new RelayCommand(UpdateShowSystemUsage);

            SaveCommand = new RelayCommand(SaveChanges);
        }

        private void SaveChanges(object parameter)
        {
            _config.PyDLLPath = PythonDLLPath.Path;
            _config.PyDLLsPath = PythonDLLsPath.Path;
            _config.ShowSystemUsage = ShowSystemUsage.Show;
            _configService.UpdateConfig(_config);
        }
        
        private void UpdatePythonDLLPath(object parameter)
        {
            if (!(parameter is string newPath))
            {
                throw new ArgumentException("Parameter must be a string!", nameof(parameter));
            }

            PythonDLLPath.Path = newPath;
            //_config.PyDLLPath = newPath;
            //_configService.UpdateConfig(_config);
        }

        private void UpdatePythonDLLsPath(object parameter)
        {
            if (!(parameter is string newPath))
            {
                throw new ArgumentException("Parameter must be a string!", nameof(parameter));
            }

            PythonDLLsPath.Path = newPath;
            //_config.PyDLLsPath = newPath;
            //_configService.UpdateConfig(_config);
        }

        private void UpdateShowSystemUsage(object parameter)
        {
            if (!(parameter is bool show))
            {
                throw new ArgumentException("Parameter must be a bool!", nameof(parameter));
            }

            ShowSystemUsage.Show = show;
            //_config.ShowSystemUsage = show;
            //_configService.UpdateConfig(_config);
        }

        public Config GetConfig() => _config;
        
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    internal class PythonDLLPath : INotifyPropertyChanged
    {
        private string _path;
        public string Path
        {
            get => _path;
            set
            {
                if (_path != value)
                {
                    _path = value;
                    OnPropertyChanged(nameof(Path));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    
    internal class PythonDLLsPath : INotifyPropertyChanged
    {
        private string _path;
        public string Path
        {
            get => _path;
            set
            {
                if (_path != value)
                {
                    _path = value;
                    OnPropertyChanged(nameof(Path));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    internal class ShowSystemUsage : INotifyPropertyChanged
    {
        private bool _show;
        public bool Show
        {
            get => _show;
            set 
            {
                if (_show != value)
                {
                    _show = value;
                    OnPropertyChanged(nameof(Show));
                }
            }
        }
        
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    
}