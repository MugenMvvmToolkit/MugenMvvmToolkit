#region Copyright

// ****************************************************************************
// <copyright file="SnackbarToastPresenter.cs">
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

using System.Threading.Tasks;
using Android.Support.Design.Widget;
using Android.Views;
using Java.Lang;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Presenters;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Android.Design.Infrastructure.Presenters
{
    public class SnackbarToastPresenter : IToastPresenter
    {
        #region Nested types

        private sealed class ToastImpl : BaseTransientBottomBar.BaseCallback, IToast
        {
            #region Fields

            private readonly TaskCompletionSource<object> _tcs;
            private Snackbar _snackbar;
            private IToast _toast;

            #endregion

            #region Constructors

            public ToastImpl()
            {
                _tcs = new TaskCompletionSource<object>();
            }

            #endregion

            #region Properties

            public Task CompletionTask => _tcs.Task;

            #endregion

            #region Methods

            public void Show(Snackbar snackbar, float duration)
            {
                _snackbar = snackbar;
                _snackbar.AddCallback(this);
                _snackbar.SetDuration((int)duration);
                _snackbar.Show();
            }

            public void FromToast(IToast toast)
            {
                _toast = toast;
                _tcs.TrySetFromTask(toast.CompletionTask);
            }

            public override void OnDismissed(Object snackbar, int evt)
            {
                base.OnDismissed(snackbar, evt);
                _tcs.TrySetResult(null);
            }

            #endregion

            #region Implementation of interfaces

            public void Close()
            {
                var toast = _toast;
                if (toast == null)
                {
                    var snackbar = _snackbar;
                    if (snackbar == null)
                        return;
                    _snackbar = null;
                    snackbar.Dismiss();
                }
                else
                {
                    _toast = null;
                    toast.Close();
                }
            }

            #endregion
        }

        #endregion

        #region Fields

        private readonly IToastPresenter _defaultPresenter;
        private readonly IThreadManager _threadManager;

        #endregion

        #region Constructors

        public SnackbarToastPresenter(IThreadManager threadManager, IToastPresenter defaultPresenter)
        {
            Should.NotBeNull(threadManager, nameof(threadManager));
            _threadManager = threadManager;
            _defaultPresenter = defaultPresenter;
        }

        #endregion

        #region Methods

        private void ShowInternal(ToastImpl toast, IDataTemplateSelector selector, View snackbarHolderView, object content, float duration, ToastPosition position, IDataContext context)
        {
            Snackbar snackbar;
            if (selector == null)
                snackbar = Snackbar.Make(snackbarHolderView, content.ToStringSafe("(null)"), (int)duration);
            else
                snackbar = (Snackbar)selector.SelectTemplate(content, snackbarHolderView);
            if (snackbar == null)
                toast.FromToast(_defaultPresenter.ShowAsync(content, duration, position, context));
            else
                toast.Show(snackbar, duration);
        }

        #endregion

        #region Implementation of interfaces

        public IToast ShowAsync(object content, float duration, ToastPosition position = ToastPosition.Bottom, IDataContext context = null)
        {
            View holder = null;
            var currentActivity = PlatformExtensions.CurrentActivity;
            if (currentActivity != null)
            {
                var selector = currentActivity.GetBindingMemberValue(AttachedMembersDesign.Activity.SnackbarViewSelector);
                holder = selector == null
                    ? currentActivity.GetBindingMemberValue(AttachedMembersDesign.Activity.SnackbarView)
                    : selector(content, position, context);
            }
            if (holder == null)
                return _defaultPresenter.ShowAsync(content, duration, position, context);

            var templateSelector = currentActivity.GetBindingMemberValue(AttachedMembersDesign.Activity.SnackbarTemplateSelector);
            var toastImpl = new ToastImpl();
            if (_threadManager.IsUiThread)
                ShowInternal(toastImpl, templateSelector, holder, content, duration, position, context);
            else
                _threadManager.InvokeOnUiThreadAsync(() => ShowInternal(toastImpl, templateSelector, holder, content, duration, position, context), OperationPriority.High);
            return toastImpl;
        }

        #endregion
    }
}