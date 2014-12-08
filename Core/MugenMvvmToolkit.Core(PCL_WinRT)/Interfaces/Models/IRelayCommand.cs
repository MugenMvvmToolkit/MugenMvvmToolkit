#region Copyright
// ****************************************************************************
// <copyright file="IRelayCommand.cs">
// Copyright © Vyacheslav Volkov 2012-2014
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
    /// <summary>
    ///     An extension to <see cref="ICommand"/> to provide an ability to raise changed events.
    /// </summary>
    public interface IRelayCommand : ICommand, IDisposable, ISuspendNotifications
    {
        /// <summary>
        ///     Gets the value that indicates that command has can execute handler.
        /// </summary>
        bool HasCanExecuteImpl { get; }

        /// <summary>
        ///     Specifies the execution mode for <c>Execute</c> method.
        /// </summary>
        CommandExecutionMode ExecutionMode { get; set; }

        /// <summary>
        ///     Specifies the execution mode for <c>RaiseCanExecuteChanged</c> method in <c>IRelayCommand</c>.
        /// </summary>
        ExecutionMode CanExecuteMode { get; set; }

        /// <summary>
        ///     Gets the current command notifiers.
        /// </summary>
        [NotNull]
        IList<object> GetNotifiers();

        /// <summary>
        ///     Adds the specified notifier to manage the <c>CanExecuteChanged</c> event.
        /// </summary>
        /// <param name="item">The specified notifier item.</param>
        bool AddNotifier([NotNull] object item);

        /// <summary>
        ///     Removes the specified notifier.
        /// </summary>
        /// <param name="item">The specified notifier item.</param>
        bool RemoveNotifier([NotNull] object item);

        /// <summary>
        ///     Removes all notifiers.
        /// </summary>
        void ClearNotifiers();

        /// <summary>
        ///     This method can be used to raise the CanExecuteChanged handler.
        /// </summary>
        void RaiseCanExecuteChanged();
    }
}