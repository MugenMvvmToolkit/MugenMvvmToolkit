#region Copyright

// ****************************************************************************
// <copyright file="IViewModel.cs">
// Copyright (c) 2012-2015 Vyacheslav Volkov
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
using System.Threading;
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Interfaces.ViewModels
{
    /// <summary>
    ///     Represents the base interface for all view models.
    /// </summary>
    public interface IViewModel : IDisposableObject, IObservable, ISuspendNotifications, IEventPublisher
    {
        /// <summary>
        ///     Gets a value indicating whether this view model is initialized.
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        ///     Gets the busy state of the current view model.
        /// </summary>
        bool IsBusy { get; }

        /// <summary>
        ///     Gets the information message for busy dialog.
        /// </summary>
        [CanBeNull]
        object BusyMessage { get; }

        /// <summary>
        ///     Gets the settings of the current view model.
        /// </summary>
        [NotNull]
        IViewModelSettings Settings { get; }

        /// <summary>
        ///     Gets the ioc adapter of the current view model.
        /// </summary>
        IIocContainer IocContainer { get; }

        /// <summary>
        ///     Gets the cancellation token that uses to cancel an operation when the current object will be disposed.
        /// </summary>
        CancellationToken DisposeCancellationToken { get; }

        /// <summary>
        ///     Initializes the current view model using the specified <see cref="IDataContext" />.
        /// </summary>
        /// <param name="context">
        ///     The specified <see cref="IDataContext" />.
        /// </param>
        void InitializeViewModel([NotNull] IDataContext context);

        /// <summary>
        ///     Begins to indicate that the current view model is busy.
        /// </summary>
        /// <param name="message">
        ///     The specified message for the <see cref="BusyMessage" /> property.
        /// </param>
        /// <returns>Id of the operation.</returns>
        [NotNull]
        IBusyToken BeginBusy(object message = null);

        /// <summary>
        ///     Gets the collection of busy tokens.
        /// </summary>
        [NotNull]
        IList<IBusyToken> GetBusyTokens();

        /// <summary>
        ///     Occurs when this <see cref="IViewModel" /> is initialized.
        ///     This event coincides with cases where the value of the <see cref="IsInitialized" /> property changes from false to true.
        /// </summary>
        event EventHandler<IViewModel, EventArgs> Initialized;
    }
}