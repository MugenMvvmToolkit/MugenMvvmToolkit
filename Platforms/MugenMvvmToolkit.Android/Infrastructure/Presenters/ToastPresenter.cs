#region Copyright

// ****************************************************************************
// <copyright file="ToastPresenter.cs">
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
using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Android.Views;
using Android.Widget;
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Presenters;
using MugenMvvmToolkit.Models;

#if XAMARIN_FORMS && ANDROID
namespace MugenMvvmToolkit.Xamarin.Forms.Android.Infrastructure.Presenters
#elif ANDROID
using MugenMvvmToolkit.Android.Binding;
using MugenMvvmToolkit.Binding;

namespace MugenMvvmToolkit.Android.Infrastructure.Presenters
#endif

{
    public class ToastPresenter : IToastPresenter
    {
        #region Nested types

        protected sealed class ToastWrapper : IToast
        {
            #region Fields

            private readonly TaskCompletionSource<object> _tcs;
            private Toast _toast;
            private const int TimerInterval = 1000;
            private readonly IThreadManager _threadManager;
            private Timer _delayTimer;
            private Timer _showTimer;

            #endregion

            #region Constructors

            public ToastWrapper(TaskCompletionSource<object> task, IThreadManager threadManager)
            {
                _tcs = task;
                _threadManager = threadManager;
            }

            #endregion

            #region Properties

            public Task CompletionTask => _tcs.Task;

            #endregion

            #region Methods

            public void Show(Toast toast, float duration, ToastPosition position)
            {
                Should.NotBeNull(toast, nameof(toast));
                _toast = toast;
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
                        throw new ArgumentOutOfRangeException(nameof(position));
                }
                _toast.Duration = ToastLength.Short;
                _toast.Show();
                _delayTimer = new Timer(DelayTimerCallback, this, (uint)duration, int.MaxValue);
                if (duration > 2000)
                    _showTimer = new Timer(ShowTimerCallback, this, TimerInterval, TimerInterval);
            }

            public void Close()
            {
                if (_delayTimer != null)
                    _delayTimer.Dispose();
                if (_showTimer != null)
                    _showTimer.Dispose();
                if (_toast == null)
                    return;
                _threadManager.Invoke(ExecutionMode.SynchronousOnUiThread, this, this, (w, wrapper) =>
                {
                    Toast toast = w._toast;
                    if (toast != null)
                    {
                        w._toast = null;
                        toast.Cancel();
                        w._tcs.TrySetResult(null);
                    }
                }, OperationPriority.High);
            }

            public void Cancel()
            {
                _tcs.SetResult(null);
            }

            private static void ShowTimerCallback(object state)
            {
                var closure = (ToastWrapper)state;
                closure._threadManager.Invoke(ExecutionMode.SynchronousOnUiThread, closure, closure,
                    (w, wrapper) =>
                    {
                        if (!w._tcs.Task.IsCompleted)
                        {
                            Toast toast = w._toast;
                            if (toast != null)
                                toast.Show();
                        }
                    }, OperationPriority.High);
            }

            private static void DelayTimerCallback(object state)
            {
                ((ToastWrapper)state).Close();
            }

            #endregion
        }

        #endregion

        #region Fields

        private const string ToastWrapperMember = "!@!ToastWrap@$2";
        private readonly IThreadManager _threadManager;

        #endregion

        #region Constructors

        public ToastPresenter([NotNull] IThreadManager threadManager)
        {
            Should.NotBeNull(threadManager, nameof(threadManager));
            _threadManager = threadManager;
        }

        #endregion

        #region Implementation of IToastPresenter

        public IToast ShowAsync(object content, float duration, ToastPosition position = ToastPosition.Bottom,
            IDataContext context = null)
        {
            var toast = new ToastWrapper(new TaskCompletionSource<object>(), _threadManager);
            if (_threadManager.IsUiThread)
                ShowInternal(content, duration, position, context, toast);
            else
                _threadManager.InvokeOnUiThreadAsync(() => ShowInternal(content, duration, position, context, toast),
                    OperationPriority.High);
            return toast;
        }

        #endregion

        #region Methods

        protected virtual void ShowInternal(object content, float duration, ToastPosition position, IDataContext context, ToastWrapper wrapper)
        {
#if XAMARIN_FORMS
            var ctx = global::Xamarin.Forms.Forms.Context;
#else
            var ctx = PlatformExtensions.CurrentActivity;
#endif
            if (ctx == null)
            {
                wrapper.Cancel();
                return;
            }
            Toast toast = null;
#if !XAMARIN_FORMS
            var selector = ctx.GetBindingMemberValue(AttachedMembers.Activity.ToastTemplateSelector);
            if (selector != null)
                toast = (Toast)selector.SelectTemplate(content, ctx);
#endif
            if (toast == null)
            {
                View view = GetView(content, ctx);
                toast = view == null
                    ? Toast.MakeText(ctx, content == null ? "(null)" : content.ToString(), ToastLength.Long)
                    : new Toast(ctx) { View = view };
            }

            ServiceProvider
                .AttachedValueProvider
                .AddOrUpdate(ctx, ToastWrapperMember, (c, o) =>
                {
                    wrapper.Show(toast, duration, position);
                    return wrapper;
                }, (item, value, oldValue, state) =>
                {
                    oldValue.Close();
                    return value(item, state);
                });
        }

        protected virtual View GetView(object content, Context ctx)
        {
#if XAMARIN_FORMS
            return null;
#else
            if (content == null || content is string)
                return null;
            var view = PlatformExtensions.GetContentView(ctx, ctx, content, null, null) as View;
            if (view != null)
                view.SetDataContext(content);
            return view;
#endif
        }

        #endregion
    }
}
