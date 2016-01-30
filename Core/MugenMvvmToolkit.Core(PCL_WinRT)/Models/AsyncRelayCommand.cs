#region Copyright

// ****************************************************************************
// <copyright file="AsyncRelayCommand.cs">
// Copyright (c) 2012-2016 Vyacheslav Volkov
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
using System.Threading.Tasks;
using JetBrains.Annotations;
using MugenMvvmToolkit.Annotations;

namespace MugenMvvmToolkit.Models
{
    public class AsyncRelayCommand : RelayCommand
    {
        #region Constructors

        public AsyncRelayCommand([NotNull] Func<Task> execute, [CanBeNull] Func<bool> canExecute, bool allowMultipleExecution,
            [NotEmptyParams] params object[] notifiers)
            : base(execute, canExecute, allowMultipleExecution, notifiers)
        {
        }

        public AsyncRelayCommand(Func<Task> execute, Func<bool> canExecute, [NotEmptyParams] params object[] notifiers)
            : base(execute, canExecute, true, notifiers)
        {
        }

        public AsyncRelayCommand(Func<Task> execute, bool allowMultipleExecution = true)
            : base(execute, null, allowMultipleExecution, null)
        {
        }

        #endregion
    }

    public class AsyncRelayCommand<TArg> : RelayCommand<TArg>
    {
        #region Constructors

        public AsyncRelayCommand([NotNull] Func<TArg, Task> execute, [CanBeNull] Func<TArg, bool> canExecute, bool allowMultipleExecution,
            [NotEmptyParams] params object[] notifiers)
            : base(execute, canExecute, allowMultipleExecution, notifiers)
        {
        }

        public AsyncRelayCommand(Func<TArg, Task> execute, Func<TArg, bool> canExecute, [NotEmptyParams] params object[] notifiers)
            : base(execute, canExecute, true, notifiers)
        {
        }

        public AsyncRelayCommand(Func<TArg, Task> execute, bool allowMultipleExecution = true)
            : base(execute, null, allowMultipleExecution, null)
        {
        }

        #endregion
    }
}