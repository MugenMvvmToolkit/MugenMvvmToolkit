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
using Windows.UI.Core;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.WinRT.Infrastructure
{
    public class ThreadManager : IThreadManager
    {
        #region Fields

        private readonly CoreDispatcher _dispatcher;

        #endregion

        #region Constructors

        public ThreadManager(CoreDispatcher dispatcher)
        {
            Should.NotBeNull(dispatcher, nameof(dispatcher));
            _dispatcher = dispatcher;
        }

        #endregion

        #region Properties

        public CoreDispatcher Dispatcher
        {
            get { return _dispatcher; }
        }

        #endregion

        #region Implementation of IThreadManager

        public bool IsUiThread
        {
            get { return Dispatcher.HasThreadAccess; }
        }

        public void InvokeOnUiThreadAsync(Action action, OperationPriority priority = OperationPriority.Normal,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Should.NotBeNull(action, nameof(action));
            if (cancellationToken.IsCancellationRequested)
                return;
            if (priority != OperationPriority.Low && IsUiThread)
                action();
            else
                Dispatcher
                    .RunAsync(ConvertToDispatcherPriority(priority), new DispatchedHandler(action))
                    .AsTask(cancellationToken);
        }

        public void InvokeOnUiThread(Action action, OperationPriority priority = OperationPriority.Normal,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Should.NotBeNull(action, nameof(action));
            if (cancellationToken.IsCancellationRequested)
                return;
            if (IsUiThread)
                action();
            else
                Dispatcher
                    .RunAsync(ConvertToDispatcherPriority(priority), new DispatchedHandler(action))
                    .AsTask(cancellationToken)
                    .Wait(cancellationToken);
        }

        public void InvokeAsync(Action action, OperationPriority priority = OperationPriority.Normal,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Should.NotBeNull(action, nameof(action));
            Should.NotBeNull(action, nameof(action));
            Task.Factory.StartNew(action, cancellationToken);
        }

        #endregion

        #region Methods

        private static CoreDispatcherPriority ConvertToDispatcherPriority(OperationPriority priority)
        {
            switch (priority)
            {
                case OperationPriority.Low:
                    return CoreDispatcherPriority.Low;
                case OperationPriority.Normal:
                    return CoreDispatcherPriority.Normal;
                case OperationPriority.High:
                    return CoreDispatcherPriority.High;
                default:
                    return CoreDispatcherPriority.Normal;
            }
        }

        #endregion
    }
}
