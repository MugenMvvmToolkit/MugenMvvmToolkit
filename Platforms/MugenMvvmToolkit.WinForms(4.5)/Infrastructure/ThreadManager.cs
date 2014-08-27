#region Copyright
// ****************************************************************************
// <copyright file="ThreadManager.cs">
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
using System.Threading.Tasks;
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Infrastructure
{
    public class ThreadManager : IThreadManager
    {
        #region Fields

        private readonly SynchronizationContext _synchronizationContext;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ThreadManager" /> class.
        /// </summary>
        public ThreadManager([NotNull] SynchronizationContext synchronizationContext)
        {
            Should.NotBeNull(synchronizationContext, "synchronizationContext");
            _synchronizationContext = synchronizationContext;
        }

        #endregion

        #region Implementation of IThreadManager

        /// <summary>
        ///     Determines whether the calling thread is the UI thread.
        /// </summary>
        /// <returns><c>true</c> if the calling thread is the UI thread; otherwise, <c>false</c>.</returns>
        public bool IsUiThread
        {
            get { return SynchronizationContext.Current == _synchronizationContext; }
        }

        /// <summary>
        ///     Invokes an action on the UI thread synchronous using the UiDispatcher.
        /// </summary>
        /// <param name="action">
        ///     The specified <see cref="System.Action" />.
        /// </param>
        /// <param name="priority">The specified <see cref="OperationPriority" /> to invoke the action.</param>
        /// <param name="cancellationToken">An object that indicates whether to cancel the operation.</param>
        public void InvokeOnUiThread(Action action, OperationPriority priority = OperationPriority.Normal,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Should.NotBeNull(action, "action");
            if (cancellationToken.IsCancellationRequested)
                return;
            if (IsUiThread)
                action();
            else
                _synchronizationContext.Send(state => ((Action)state).Invoke(), action);
        }

        /// <summary>
        ///     Invokes an action on the UI thread using the UiDispatcher.
        /// </summary>
        /// <param name="action">
        ///     The specified <see cref="System.Action" />.
        /// </param>
        /// <param name="priority">The specified <see cref="OperationPriority" /> to invoke the action.</param>
        /// <param name="cancellationToken">An object that indicates whether to cancel the operation.</param>
        public void InvokeOnUiThreadAsync(Action action, OperationPriority priority = OperationPriority.Normal,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Should.NotBeNull(action, "action");
            if (cancellationToken.IsCancellationRequested)
                return;
            if (priority != OperationPriority.Low && IsUiThread)
                action();
            else
                _synchronizationContext.Post(state => ((Action)state).Invoke(), action);
        }

        /// <summary>
        ///     Invokes an action asynchronous.
        /// </summary>
        /// <param name="action">
        ///     The specified <see cref="System.Action" />.
        /// </param>
        /// <param name="priority">The specified <see cref="OperationPriority" /> to invoke the action.</param>
        /// <param name="cancellationToken">An object that indicates whether to cancel the operation.</param>
        public void InvokeAsync(Action action, OperationPriority priority = OperationPriority.Normal,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Should.NotBeNull(action, "action");
            Task.Factory.StartNew(action, cancellationToken);
        }

        #endregion
    }
}