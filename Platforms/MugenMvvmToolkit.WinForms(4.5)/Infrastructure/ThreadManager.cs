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
using JetBrains.Annotations;
using MugenMvvmToolkit.Attributes;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Models;

#if WINFORMS
namespace MugenMvvmToolkit.WinForms.Infrastructure
#elif ANDROID
namespace MugenMvvmToolkit.Android.Infrastructure
#elif TOUCH
namespace MugenMvvmToolkit.iOS.Infrastructure
#elif XAMARIN_FORMS
namespace MugenMvvmToolkit.Xamarin.Forms.Infrastructure
#endif

{
    public class ThreadManager : IThreadManager
    {
        #region Fields

        private readonly SynchronizationContext _synchronizationContext;
        private int? _mainThreadId;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public ThreadManager([NotNull] SynchronizationContext synchronizationContext)
        {
            Should.NotBeNull(synchronizationContext, nameof(synchronizationContext));
            _synchronizationContext = synchronizationContext;
            synchronizationContext.Post(state => ((ThreadManager)state)._mainThreadId = ManagedThreadId, this);
            ToolkitServiceProvider.UiSynchronizationContext = synchronizationContext;
        }

        #endregion

        #region Properties

        private static int ManagedThreadId
        {
            get
            {
#if XAMARIN_FORMS
                return Environment.CurrentManagedThreadId;
#else
                return Thread.CurrentThread.ManagedThreadId;
#endif
            }
        }

        #endregion

        #region Implementation of IThreadManager

        public bool IsUiThread
        {
            get
            {
                if (_mainThreadId == null)
                    return SynchronizationContext.Current == _synchronizationContext;
                return _mainThreadId.Value == ManagedThreadId;
            }
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
                _synchronizationContext.Send(state => ((Action)state).Invoke(), action);
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
                _synchronizationContext.Post(state => ((Action)state).Invoke(), action);
        }

        public void InvokeAsync(Action action, OperationPriority priority = OperationPriority.Normal,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Should.NotBeNull(action, nameof(action));
            Task.Factory.StartNew(action, cancellationToken);
        }

        #endregion
    }
}
