using Ollama_assistance.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;


namespace Ollama_assistance.Commands
{
    class sendMessageCommand : ICommand
    {
        private readonly MainViewModel _viewModel;

        public sendMessageCommand(MainViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public bool CanExecute(object parameter)
        {
            return !string.IsNullOrWhiteSpace(parameter as string);
            //return parameter != null && parameter is string;
            //return !string.IsNullOrWhiteSpace(_viewModel.NewMessage);
        }

        public void Execute(object parameter)
        {
            if (CanExecute(parameter))
            {
                _viewModel.SendMessage(parameter as string);
            }
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove {  CommandManager.RequerySuggested -= value; }
        }
    }
}
