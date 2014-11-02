#region Copyright
// ****************************************************************************
// <copyright file="ToastPresenter.cs">
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
using Android.Content;
using Android.Views;
using Android.Widget;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Navigation;
using MugenMvvmToolkit.Interfaces.Presenters;
using MugenMvvmToolkit.Interfaces.Views;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Infrastructure.Presenters
{
    /// <summary>
    ///     Provides functionality to present a timed message.
    /// </summary>
    public class ToastPresenter : IToastPresenter
    {
        #region Nested types

        private sealed class ToastWrapper
        {
            #region Fields

            private const int TimerInterval = 1000;
            private readonly TaskCompletionSource<object> _task;
            private readonly IThreadManager _threadManager;
            private Timer _delayTimer;
            private Timer _showTimer;
            private Toast _toast;

            #endregion

            #region Constructors

            public ToastWrapper(Toast toast, TaskCompletionSource<object> task, IThreadManager threadManager)
            {
                _toast = toast;
                _task = task;
                _threadManager = threadManager;
            }

            #endregion

            #region Methods

            public void Show(float duration, ToastPosition position)
            {
                switch (position)
                {
                    case ToastPosition.Bottom:
                        _toast.SetGravity(GravityFlags.Bottom | GravityFlags.CenterHorizontal, 0, 0);
                        break;
                    case ToastPosition.Center:
                        _toast.SetGravity(GravityFlags.Center, 0, 0);
                        break;
                    case ToastPosition.Top:
                        _toast.SetGravity(GravityFlags.Top | GravityFlags.CenterHorizontal, 0, 0);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("position");
                }
                _toast.Duration = ToastLength.Short;
                _toast.Show();
                _delayTimer = new Timer(DelayTimerCallback, this, (uint)duration, int.MaxValue);
                if (duration > 2000)
                    _showTimer = new Timer(ShowTimerCallback, this, TimerInterval, TimerInterval);
            }

            public void Complete()
            {
                if (_delayTimer != null)
                    _delayTimer.Dispose();
                if (_showTimer != null)
                    _showTimer.Dispose();
                _threadManager.InvokeOnUiThread(() =>
                {
                    Toast toast = _toast;
                    if (toast != null)
                    {
                        _toast = null;
                        toast.Cancel();
                        _task.TrySetResult(null);
                    }
                }, OperationPriority.High);
            }

            private static void ShowTimerCallback(object state)
            {
                var closure = (ToastWrapper)state;
                closure._threadManager.InvokeOnUiThread(() =>
                {
                    if (!closure._task.Task.IsCompleted)
                    {
                        Toast toast = closure._toast;
                        if (toast != null)
                            toast.Show();
                    }
                }, OperationPriority.High);
            }

            private static void DelayTimerCallback(object state)
            {
                ((ToastWrapper)state).Complete();
            }

            #endregion
        }

        #endregion

        #region Fields

        private const string ToastWrapperMember = "!@!ToastWrap@$2";
        private const string DisposedEventHandler = "3w4toasthandler3w5rews";
        private readonly INavigationProvider _navigationProvider;
        private readonly IThreadManager _threadManager;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ToastPresenter" /> class.
        /// </summary>
        public ToastPresenter([NotNull] INavigationProvider navigationProvider,
            [NotNull] IThreadManager threadManager)
        {
            Should.NotBeNull(navigationProvider, "navigationProvider");
            Should.NotBeNull(threadManager, "threadManager");
            _navigationProvider = navigationProvider;
            _threadManager = threadManager;
        }

        #endregion

        #region Implementation of IToastPresenter

        /// <summary>
        ///     Shows the specified message.
        /// </summary>
        public Task ShowAsync(object content, float duration, ToastPosition position = ToastPosition.Bottom,
            IDataContext context = null)
        {
            var tcs = new TaskCompletionSource<object>();
            if (_threadManager.IsUiThread)
                ShowInternal(content, duration, position, context, tcs);
            else
                _threadManager.InvokeOnUiThreadAsync(() => ShowInternal(content, duration, position, context, tcs),
                    OperationPriority.High);
            return tcs.Task;
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Shows the specified message.
        /// </summary>
        protected virtual void ShowInternal(object content, float duration, ToastPosition position, IDataContext context,
            TaskCompletionSource<object> tcs)
        {
            var ctx = _navigationProvider.CurrentContent as Context;
            if (ctx == null)
            {
                tcs.SetResult(null);
                return;
            }
            View view = GetView(content, ctx);
            Toast toast = view == null
                ? Toast.MakeText(ctx, content == null ? "(null)" : content.ToString(), ToastLength.Long)
                : new Toast(ctx) { View = view };

            var wrapper = new ToastWrapper(toast, tcs, _threadManager);
            ServiceProvider
                .AttachedValueProvider
                .AddOrUpdate(ctx, ToastWrapperMember, (c, o) =>
                {
                    wrapper.Show(duration, position);
                    return wrapper;
                }, (item, value, oldValue, state) =>
                {
                    oldValue.Complete();
                    return value(item, state);
                });
            var activityView = ctx as IActivityView;
            if (activityView == null)
                return;
            ServiceProvider.AttachedValueProvider.GetOrAdd(activityView, DisposedEventHandler, (view1, o) =>
            {
                view1.Destroyed += ActivityOnDestroyed;
                return DisposedEventHandler;
            }, null);
        }

        protected virtual View GetView(object content, Context ctx)
        {
            if (content == null || content is string)
                return null;
            var view = PlatformExtensions.GetContentView(ctx, ctx, content, null, null) as View;
            if (view != null)
                BindingServiceProvider.ContextManager.GetBindingContext(view).Value = content;
            return view;
        }

        private static void ActivityOnDestroyed(object sender, EventArgs eventArgs)
        {
            var wrapper = ServiceProvider.AttachedValueProvider.GetValue<ToastWrapper>(sender, ToastWrapperMember, false);
            if (wrapper != null)
                wrapper.Complete();
        }

        #endregion
    }
}