using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Diagnostics.Contracts;

namespace Utils
{
    public class DelegateCommand : ICommand
    {
        private readonly Func<object, bool> canExecuteFunc;
        private readonly Action<object> executeAction;

        public DelegateCommand(Action<object> execute)
            : this(execute, (x) => true)
        {
        }

        public DelegateCommand(Action<object> execute, Func<object, bool> canExecute)
        {
            Contract.Requires(execute != null);
            Contract.Requires(canExecute != null);

            canExecuteFunc = canExecute;
            executeAction = execute;
        }

        public void FireCanExecuteChanged()
        {
            if (CanExecuteChanged != null)
                CanExecuteChanged(this, EventArgs.Empty);
        }

        public bool CanExecute(object parameter)
        {
            return canExecuteFunc(parameter);
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            executeAction(parameter);
        }
    }
}
