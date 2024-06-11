using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using Ollama_assistance.Services;
using Ollama_assistance.Utils;

namespace Ollama_assistance.ViewModel
{
    class ConfigViewModel
    {
        private static ConfigService _configService;
        private Config _config;
        
        public ObservableCollection<PythonDLLPath> PythonDLLPaths { get; set; }
        public ObservableCollection<PythonDLLsPath> PythonDLLsPaths { get; set; }
        public ObservableCollection<ShowSystemUsage> ShowSystemUsages { get; set; }
        
        public ICommand UpdatePythonDLLPathCommand { get; set; }
        public ICommand UpdatePythonDLLsPathCommand { get; set; }
        public ICommand UpdateShowSystemUsageCommand { get; set; }
        
        public ConfigViewModel()
        {
            _configService = new ConfigService();
            _config = _configService.getConfig();
            
            PythonDLLPaths = new ObservableCollection<PythonDLLPath>();
            PythonDLLsPaths = new ObservableCollection<PythonDLLsPath>();
            ShowSystemUsages = new ObservableCollection<ShowSystemUsage>();
            
            UpdatePythonDLLPathCommand = new RelayCommand(UpdatePythonDLLPath);
            UpdatePythonDLLsPathCommand = new RelayCommand(UpdatePythonDLLsPath);
            UpdateShowSystemUsageCommand = new RelayCommand(UpdateShowSystemUsage);
        }
        
        private void UpdatePythonDLLPath(object parameter)
        {
            if (!(parameter is string newPath))
            {
                throw new ArgumentException("Parameter must be a string!", nameof(parameter));
            }
            _config.PyDLLPath = newPath;
            _configService.UpdateConfig(_config);
        }

        private void UpdatePythonDLLsPath(object parameter)
        {
            if (!(parameter is string newPath))
            {
                throw new ArgumentException("Parameter must be a string!", nameof(parameter));
            }
            _config.PyDLLsPath = newPath;
            _configService.UpdateConfig(_config);
        }

        private void UpdateShowSystemUsage(object parameter)
        {
            if (!(parameter is bool show))
            {
                throw new ArgumentException("Parameter must be a bool!", nameof(parameter));
            }
            _config.ShowSystemUsage = show;
            _configService.UpdateConfig(_config);
        }

        public Config GetConfig() => _config;
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