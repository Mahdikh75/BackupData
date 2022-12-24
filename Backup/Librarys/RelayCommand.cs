using System;
using System.Windows.Input;

namespace Backup.Librarys
{
    public class RelayCommand : ICommand
    {
        public readonly Action<object> execute;
        public readonly Predicate<object> can_execute;

        public RelayCommand(Action<object> execute) : this(execute, null) { }

        public RelayCommand(Action<object> execute, Predicate<object> can_execute)
        {
            if (execute != null)
            {
                this.execute = execute;
                this.can_execute = can_execute;
            }
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested += value; }
        }

        public bool CanExecute(object parameter)
        {
            return can_execute == null ? true : can_execute(parameter);
        }

        public void Execute(object parameter)
        {
            execute(parameter);
        }
    }
}