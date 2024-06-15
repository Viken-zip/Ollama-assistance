using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Forms;
using Ollama_assistance.Utils;
using Ollama_assistance.Services;

namespace Ollama_assistance.ViewModel
{
    class MainViewModel : INotifyPropertyChanged
    {
        private ConfigService _configService;
        private Config _config;
        
        public ObservableCollection<DisplayOption> Displays { get; set; }
        public ObservableCollection<CornerPosition> CornerPositions { get; set; }
        public ObservableCollection<string> ChatMessages { get; set; }
        
        public ICommand SelectDisplayCommand { get; }
        public ICommand SelectCornerCommand { get; }
        public ICommand SendMessageCommand { get; set; }
        

        public MainViewModel()
        {
            _configService = new ConfigService();
            _config = _configService.getConfig();
            
            Displays = new ObservableCollection<DisplayOption>();
            CornerPositions = new ObservableCollection<CornerPosition>();
            SelectDisplayCommand = new RelayCommand(SelectDisplay);
            SelectCornerCommand = new RelayCommand(SelectCorner);

            ChatMessages = new ObservableCollection<string>(ChatHistoryService.LoadChatHistory());
            SendMessageCommand = new RelayCommand(
                parameter => SendMessage(NewMessage),
                parameter => !string.IsNullOrWhiteSpace(NewMessage)
            );

            PopulateDisplays();
            PopulateCornerPositions();

            CurrentDisplayIndex = _config.CurrentDisplayIndex;
            CurrentCornerIndex = _config.CurrentCornerIndex; // 0 top left | 1 top right | 2 bottom left | 3 bottom right

            UpdateWindowPosition();
        }

        private int _currentDisplayIndex;
        private int _currentCornerIndex;

        public int CurrentDisplayIndex
        {
            get => _currentDisplayIndex;
            set
            {
                if (_currentDisplayIndex != value)
                {
                    _currentDisplayIndex = value;
                    OnPropertyChanged(nameof(CurrentDisplayIndex));
                    UpdateWindowPosition();
                }
            }
        }

        public void SendMessage(string message)
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                ChatMessages.Add(message);
                ChatHistoryService.SaveMessageToHistory(message);
                message = string.Empty;
            }
        }

        private bool CanSendMessage()
        {
            return !string.IsNullOrWhiteSpace(NewMessage);
        }
        
        public int CurrentCornerIndex
        {
            get => _currentCornerIndex;
            set
            {
                if (_currentCornerIndex != value)
                {
                    _currentCornerIndex = value;
                    OnPropertyChanged(nameof(CurrentCornerIndex));
                    UpdateWindowPosition();
                }
            }
        }

        private string _newMessage;
        public string NewMessage
        {
            get => _newMessage;
            set
            {
                _newMessage = value;
                OnPropertyChanged(nameof(NewMessage));
            }
        }
        
        private void PopulateCornerPositions()
        {
            CornerPositions.Add(new CornerPosition { Name = "Top Left", Index = 0 });
            CornerPositions.Add(new CornerPosition { Name = "Top Right", Index = 1 });
            CornerPositions.Add(new CornerPosition { Name = "Bottom Left", Index = 2 });
            CornerPositions.Add(new CornerPosition { Name = "Bottom Right", Index = 3 });
        }

        private void PopulateDisplays()
        {
            var screens = Screen.AllScreens;
            for (int i = 0; i < screens.Length; i++)
            {
                Displays.Add(new DisplayOption { Name = $"Display {i + 1}", Index = i }); //don't get confused here, i + 1 is just for simplicity for the user
            }
        }

        private void SelectDisplay(object parameter)
        {
            CurrentDisplayIndex = (int)parameter;
            _config.CurrentDisplayIndex = (int)parameter;
            _configService.UpdateConfig(_config);
        }

        private void SelectCorner(object parameter)
        {
            CurrentCornerIndex = (int)parameter;
            _config.CurrentCornerIndex = (int)parameter;
            _configService.UpdateConfig(_config);
        }

        private void UpdateWindowPosition()
        {
            var screens = Screen.AllScreens;
            Screen screen;
            if (screens.Length >= CurrentDisplayIndex)
            {
                screen = Screen.AllScreens[CurrentDisplayIndex];
            }
            else
            {
                screen = Screen.AllScreens[0];
                _config.CurrentCornerIndex = 0;
                _configService.UpdateConfig(_config);
            }

            var window = System.Windows.Application.Current.MainWindow;
            var workingArea = screen.WorkingArea;

            double newLeft = 0;
            double newTop = 0;

            switch (CurrentCornerIndex)
            {
                case 0: // Top Left
                    newLeft = workingArea.Left + 25;
                    newTop = workingArea.Top + 25;
                    break;
                case 1: // Top Right
                    newLeft = workingArea.Left + (workingArea.Width - (window.Width + 25));
                    newTop = workingArea.Top + 25;
                    break;
                case 2: // Bottom Left
                    newLeft = workingArea.Left + 25;
                    newTop = workingArea.Top + (workingArea.Height - (window.Height + 25));
                    break;
                case 3: // Bottom Right
                    newLeft = workingArea.Left + (workingArea.Width - (window.Width + 25));
                    newTop = workingArea.Top + (workingArea.Height - (window.Height + 25));
                    break;
            }

            newLeft = Math.Max(workingArea.Left, Math.Min(newLeft, workingArea.Right - window.Width));
            newTop = Math.Max(workingArea.Top, Math.Min(newTop, workingArea.Bottom - window.Height));

            window.Width = Math.Min(window.Width, workingArea.Width);
            window.Height = Math.Min(window.Height, workingArea.Height);
            
            window.Left = newLeft;
            window.Top = newTop;

            foreach (var display in Displays)
            {
                display.IsSelected = display.Index == CurrentDisplayIndex;
            }

            foreach (var corner in CornerPositions)
            {
                corner.IsSelected = corner.Index == CurrentCornerIndex;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


    }

    public class DisplayOption : INotifyPropertyChanged
    {
        private bool _isSelected;
        public string Name { get; set; }
        public int Index { get; set; }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class CornerPosition : INotifyPropertyChanged
    {
        private bool _isSelected;
        public string Name { get; set; }
        public int Index { get; set; }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
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
