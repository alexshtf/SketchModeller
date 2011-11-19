using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Diagnostics.Contracts;

namespace Utils
{
    /*
    /// <summary>
    /// An implementation of the <see cref="ICommand"/> interface for WPF commands with user supplied delegates.
    /// </summary>
    public class DelegateCommand : ICommand
    {
        private readonly Func<object, bool> canExecuteFunc;
        private readonly Action<object> executeAction;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegateCommand"/> class given an execution delegate. This
        /// command will always return <c>true</c> from its <see cref="CanExecute"/> method.
        /// </summary>
        /// <param name="execute">The command execution delegate.</param>
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

        /// <summary>
        /// Fires the <see cref="CanExecuteChanged"/> event.
        /// </summary>
        public void FireCanExecuteChanged()
        {
            if (CanExecuteChanged != null)
                CanExecuteChanged(this, EventArgs.Empty);
        }

        /// <summary>
        /// Defines the method that determines whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
        /// <returns>
        /// true if this command can be executed; otherwise, false.
        /// </returns>
        public bool CanExecute(object parameter)
        {
            return canExecuteFunc(parameter);
        }

        /// <summary>
        /// Occurs when changes occur that affect whether or not the command should execute.
        /// </summary>
        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// Defines the method to be called when the command is invoked.
        /// </summary>
        /// <param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
        public void Execute(object parameter)
        {
            executeAction(parameter);
        }
    }
     * */
}
