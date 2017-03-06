#region Copyright

// ****************************************************************************
// <copyright file="ThreadManager.cs">
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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using MugenMvvmToolkit.Attributes;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.WPF.Infrastructure
{
    public class ThreadManager : IThreadManager
    {
        #region Fields

        private readonly Dispatcher _dispatcher;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public ThreadManager(Dispatcher dispatcher)
        {
            Should.NotBeNull(dispatcher, nameof(dispatcher));
            _dispatcher = dispatcher;
        }

        #endregion

        #region Properties

        public Dispatcher Dispatcher => _dispatcher;

        #endregion

        #region Implementation of IThreadManager

        public bool IsUiThread => Dispatcher.CheckAccess();

        public void InvokeOnUiThreadAsync(Action action, OperationPriority priority = OperationPriority.Normal,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Should.NotBeNull(action, nameof(action));
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

        public void InvokeOnUiThread(Action action, OperationPriority priority = OperationPriority.Normal,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Should.NotBeNull(action, nameof(action));
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

        public void InvokeAsync(Action action, OperationPriority priority = OperationPriority.Normal,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Should.NotBeNull(action, nameof(action));
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
