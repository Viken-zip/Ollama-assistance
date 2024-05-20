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

namespace Ollama_assistance.ViewModel
{
    class MainViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<DisplayOption> Displays { get; set; }
        public ICommand SelectDisplayCommand { get; }

        public MainViewModel()
        {
            Displays = new ObservableCollection<DisplayOption>();
            SelectDisplayCommand = new RelayCommand(SelectDisplay);
            PopulateDisplays();

            SelectDisplay(0);
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
            int displayIndex = (int)parameter;
            var screen = Screen.AllScreens[displayIndex];
            var window = System.Windows.Application.Current.MainWindow;

            System.Drawing.Rectangle workingArea = screen.WorkingArea;

            window.Width = workingArea.Width / 5;
            window.Height = workingArea.Height / 2;
            window.Left = workingArea.Left + (workingArea.Width - (window.Width + 25));
            window.Top = workingArea.Top + (workingArea.Height - (window.Height + 25));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    public class DisplayOption
    {
        public string Name { get; set; }
        public int Index { get; set; }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;

        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}
