#region Copyright

// ****************************************************************************
// <copyright file="SnackbarToastPresenter.cs">
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

using System.Threading.Tasks;
using Android.App;
using Android.Support.Design.Widget;
using Android.Views;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Presenters;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Android.Design.Infrastructure.Presenters
{
    public class SnackbarToastPresenter : IToastPresenter
    {
        #region Nested types

        private sealed class ToastImpl : Snackbar.Callback, IToast
        {
            #region Fields

            private readonly TaskCompletionSource<object> _tcs;
            private Snackbar _snackbar;

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
                _snackbar.SetCallback(this).SetDuration((int)duration).Show();
            }

            public override void OnDismissed(Snackbar snackbar, int evt)
            {
                base.OnDismissed(snackbar, evt);
                _tcs.TrySetResult(null);
            }

            #endregion

            #region Implementation of interfaces

            public void Close()
            {
                var snackbar = _snackbar;
                if (snackbar == null)
                    return;
                _snackbar = null;
                snackbar.Dismiss();
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

        private static void ShowInternal(Activity activity, View snackbarHolderView, object content, float duration, ToastImpl toast)
        {
            Snackbar snackbar = null;
            var selector = activity.GetBindingMemberValue(AttachedMembersDesign.Activity.SnackbarTemplateSelector);
            if (selector != null)
                snackbar = (Snackbar)selector.SelectTemplate(content, snackbarHolderView);
            if (snackbar == null)
                snackbar = Snackbar.Make(snackbarHolderView, content.ToStringSafe("(null)"), (int)duration);
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
                if (selector != null)
                    holder = selector(content, position, context);
                if (holder == null)
                    holder = currentActivity.GetBindingMemberValue(AttachedMembersDesign.Activity.SnackbarView);
            }
            if (holder == null)
                return _defaultPresenter.ShowAsync(content, duration, position, context);
            var toastImpl = new ToastImpl();
            if (_threadManager.IsUiThread)
                ShowInternal(currentActivity, holder, content, duration, toastImpl);
            else
                _threadManager.InvokeOnUiThreadAsync(() => ShowInternal(currentActivity, holder, content, duration, toastImpl), OperationPriority.High);
            return toastImpl;
        }

        #endregion
    }
}