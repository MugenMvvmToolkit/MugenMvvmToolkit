#region Copyright

// ****************************************************************************
// <copyright file="RelayCommandMock.cs">
// Copyright (c) 2012-2017 Vyacheslav Volkov
// </copyright>
// ****************************************************************************
// <author>Vyacheslav Volkov</author>
// <email>vvs0205@outlook.com</email>
// <project>MugenMvvmToolkit</project>
// <web>https://github.com/MugenMvvmToolkit/MugenMvvmToolkit</web>
// <license>
// See license.txt in this solution or http://opensource.org/licenses/MS-PL
// </license>
// ****************************************************************************

#endregion

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

        public bool HasCanExecuteImpl => true;

        public CommandExecutionMode ExecutionMode { get; set; }

        public ExecutionMode CanExecuteMode { get; set; }

        public bool IsExecuting { get; set; }

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
