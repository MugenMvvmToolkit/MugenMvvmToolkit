using System;
using System.Collections.Generic;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Test.TestModels
{
    public class RelayCommandMock : NotifyPropertyChangedBase, IRelayCommand
    {
        #region Properties

        public bool IsDisposed { get; set; }

        #endregion

        #region Implementation of ICommand

        /// <summary>
        ///     Defines the method to be called when the command is invoked.
        /// </summary>
        /// <param name="parameter">
        ///     Data used by the command.  If the command does not require data to be passed, this object can
        ///     be set to null.
        /// </param>
        public void Execute(object parameter)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        ///     Defines the method that determines whether the command can execute in its current state.
        /// </summary>
        /// <returns>
        ///     true if this command can be executed; otherwise, false.
        /// </returns>
        /// <param name="parameter">
        ///     Data used by the command.  If the command does not require data to be passed, this object can
        ///     be set to null.
        /// </param>
        public bool CanExecute(object parameter)
        {
            throw new NotSupportedException();
        }

        public event EventHandler CanExecuteChanged;

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            IsDisposed = true;
        }

        /// <summary>
        ///     Specifies the execution mode for <c>Execute</c> method.
        /// </summary>
        public CommandExecutionMode ExecutionMode { get; set; }

        /// <summary>
        ///     Specifies the execution mode for <c>RaiseCanExecuteChanged</c> method in <c>IRelayCommand</c>.
        /// </summary>
        public ExecutionMode CanExecuteMode { get; set; }

        /// <summary>
        ///     Gets the current command notifiers.
        /// </summary>
        public IList<object> GetNotifiers()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        ///     Adds the specified notifier to manage the <c>CanExecuteChanged</c> event.
        /// </summary>
        /// <param name="item">The specified notifier item.</param>
        public bool AddNotifier(object item)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        ///     Removes the specified notifier.
        /// </summary>
        /// <param name="item">The specified notifier item.</param>
        public bool RemoveNotifier(object item)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        ///     Removes all notifiers.
        /// </summary>
        public void ClearNotifiers()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        ///     This method can be used to raise the CanExecuteChanged handler.
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            EventHandler handler = CanExecuteChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        #endregion
    }
}