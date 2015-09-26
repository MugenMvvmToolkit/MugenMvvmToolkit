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

        public void Execute(object parameter)
        {
            throw new NotSupportedException();
        }

        public bool CanExecute(object parameter)
        {
            throw new NotSupportedException();
        }

        public event EventHandler CanExecuteChanged;

        public void Dispose()
        {
            IsDisposed = true;
        }

        public bool HasCanExecuteImpl
        {
            get { return true; }
        }

        public CommandExecutionMode ExecutionMode { get; set; }

        public ExecutionMode CanExecuteMode { get; set; }

        public IList<object> GetNotifiers()
        {
            throw new NotSupportedException();
        }

        public bool AddNotifier(object item)
        {
            throw new NotSupportedException();
        }

        public bool RemoveNotifier(object item)
        {
            throw new NotSupportedException();
        }

        public void ClearNotifiers()
        {
            throw new NotSupportedException();
        }

        public void RaiseCanExecuteChanged()
        {
            EventHandler handler = CanExecuteChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        #endregion
    }
}
