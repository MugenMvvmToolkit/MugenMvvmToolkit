#region Copyright
// ****************************************************************************
// <copyright file="SynchronousThreadManager.cs">
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
using System.Threading;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Infrastructure
{
    /// <summary>
    ///     Represents the implemenation of <see cref="IThreadManager" /> that can be used for unit-test, all action invokes in
    ///     the same thread.
    /// </summary>
    public sealed class SynchronousThreadManager : IThreadManager
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="SynchronousThreadManager" /> class.
        /// </summary>
        public SynchronousThreadManager()
        {
            IsUiThread = true;
        }

        #endregion

        #region Implementation of IThreadManager

        /// <summary>
        ///     Determines whether the calling thread is the UI thread.
        /// </summary>
        /// <returns><c>true</c> if the calling thread is the UI thread; otherwise, <c>false</c>.</returns>
        public bool IsUiThread { get; set; }

        /// <summary>
        ///     Invokes an action on the UI thread using the UiDispatcher.
        /// </summary>
        /// <param name="action">
        ///     The specified <see cref="Action" />.
        /// </param>
        /// <param name="priority">The specified <see cref="OperationPriority" /> to invoke the action.</param>
        /// <param name="cancellationToken">An object that indicates whether to cancel the operation.</param>
        public void InvokeOnUiThreadAsync(Action action, OperationPriority priority = OperationPriority.Normal,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            InvokeOnUiThread(action, priority, cancellationToken);
        }

        /// <summary>
        ///     Invokes an action on the UI thread synchronous using the UiDispatcher.
        /// </summary>
        /// <param name="action">
        ///     The specified <see cref="Action" />.
        /// </param>
        /// <param name="priority">The specified <see cref="OperationPriority" /> to invoke the action.</param>
        /// <param name="cancellationToken">An object that indicates whether to cancel the operation.</param>
        public void InvokeOnUiThread(Action action, OperationPriority priority = OperationPriority.Normal,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Should.NotBeNull(action, "action");
            if (!cancellationToken.IsCancellationRequested)
                action();
        }

        /// <summary>
        ///     Invokes an action asynchronous.
        /// </summary>
        /// <param name="action">
        ///     The specified <see cref="Action" />.
        /// </param>
        /// <param name="priority">The specified <see cref="OperationPriority" /> to invoke the action.</param>
        /// <param name="cancellationToken">An object that indicates whether to cancel the operation.</param>
        public void InvokeAsync(Action action, OperationPriority priority = OperationPriority.Normal,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            InvokeOnUiThread(action, priority, cancellationToken);
        }

        #endregion
    }
}