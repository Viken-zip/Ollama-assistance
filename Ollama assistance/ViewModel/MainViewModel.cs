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
        // position of the application
        public ObservableCollection<DisplayOption> Displays { get; set; }
        public ObservableCollection<CornerPosition> CornerPositions { get; set; }
        public ICommand SelectDisplayCommand { get; }
        public ICommand SelectCornerCommand { get; }
        public ICommand SendMessageCommand { get; set; }

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

        //corner to render application shinanigans
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

        public ObservableCollection<string> ChatMessages { get; set; }

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

        public MainViewModel()
        {
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

            CurrentDisplayIndex = 0;
            CurrentCornerIndex = 3; // 0 top left | 1 top right | 2 bottom left | 3 bottom right

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
                Displays.Add(new DisplayOption { Name = $"Display {i + 1}", Index = i });
            }
        }

        private void SelectDisplay(object parameter)
        {
            CurrentDisplayIndex = (int)parameter;
        }

        private void SelectCorner(object parameter)
        {
            CurrentCornerIndex = (int)parameter;
        }

        private void UpdateWindowPosition()
        {
            var screen = Screen.AllScreens[CurrentDisplayIndex];
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

            window.Width = workingArea.Width / 5;
            window.Height = workingArea.Height / 2;
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
