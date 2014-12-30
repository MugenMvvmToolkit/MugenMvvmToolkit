#region Copyright

// ****************************************************************************
// <copyright file="ThreadManager.cs">
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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Infrastructure
{
    public class ThreadManager : IThreadManager
    {
        #region Fields

        private readonly Dispatcher _dispatcher;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ThreadManager" /> class.
        /// </summary>
        public ThreadManager(Dispatcher dispatcher)
        {
            Should.NotBeNull(dispatcher, "dispatcher");
            _dispatcher = dispatcher;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the current dispatcher.
        /// </summary>
        public Dispatcher Dispatcher
        {
            get { return _dispatcher; }
        }

        #endregion

        #region Implementation of IThreadManager

        /// <summary>
        ///     Determines whether the calling thread is the UI thread.
        /// </summary>
        /// <returns><c>true</c> if the calling thread is the UI thread; otherwise, <c>false</c>.</returns>
        public bool IsUiThread
        {
            get { return Dispatcher.CheckAccess(); }
        }

        /// <summary>
        ///     Invokes an action on the UI thread.
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
            {
                action();
                return;
            }
#if WPF
#if NET45
            Dispatcher.InvokeAsync(action, ConvertToDispatcherPriority(priority), cancellationToken);
#else
            Dispatcher.BeginInvoke(action, ConvertToDispatcherPriority(priority));
#endif
#else
            Dispatcher.BeginInvoke(action);
#endif
        }

        /// <summary>
        ///     Invokes an action on the UI thread synchronous.
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
            {
                action();
                return;
            }
#if WPF
            Dispatcher.Invoke(action, ConvertToDispatcherPriority(priority), cancellationToken);
#else
            using (EventWaitHandle wait = new AutoResetEvent(false))
            {
                Dispatcher.BeginInvoke(() =>
                {
                    action();
                    wait.Set();
                });
                wait.WaitOne();
            }
#endif
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

        #region Methods

#if WPF
        private static DispatcherPriority ConvertToDispatcherPriority(OperationPriority priority)
        {
            switch (priority)
            {
                case OperationPriority.Low:
                    return DispatcherPriority.Background;
                case OperationPriority.Normal:
                    return DispatcherPriority.Normal;
                case OperationPriority.High:
                    return DispatcherPriority.Send;
                default:
                    return DispatcherPriority.Normal;
            }
        }
#endif

        #endregion
    }
}