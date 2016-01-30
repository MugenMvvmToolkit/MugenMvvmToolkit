#region Copyright

// ****************************************************************************
// <copyright file="IRelayCommand.cs">
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
using System.Collections.Generic;
using System.Windows.Input;
using JetBrains.Annotations;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Interfaces.Models
{
    public interface IRelayCommand : ICommand, IDisposable, ISuspendNotifications
    {
        bool HasCanExecuteImpl { get; }

        CommandExecutionMode ExecutionMode { get; set; }

        ExecutionMode CanExecuteMode { get; set; }

        [NotNull]
        IList<object> GetNotifiers();

        bool AddNotifier([NotNull] object item);

        bool RemoveNotifier([NotNull] object item);

        void ClearNotifiers();

        void RaiseCanExecuteChanged();
    }
}
