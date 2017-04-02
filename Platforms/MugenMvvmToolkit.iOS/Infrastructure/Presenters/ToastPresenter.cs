#region Copyright

// ****************************************************************************
// <copyright file="ToastPresenter.cs">
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
using System.Linq;
using System.Threading.Tasks;
using Foundation;
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Presenters;
using MugenMvvmToolkit.Models;
using UIKit;

#if XAMARIN_FORMS
using MugenMvvmToolkit.Xamarin.Forms.iOS.Views;

namespace MugenMvvmToolkit.Xamarin.Forms.iOS.Infrastructure.Presenters
#else
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.iOS.Binding;
using MugenMvvmToolkit.iOS.Views;

namespace MugenMvvmToolkit.iOS.Infrastructure.Presenters
#endif
{
    public class ToastPresenter : IToastPresenter
    {
        #region Nested types

        private sealed class ToastImpl : IToast
        {
            #region Fields

            public readonly TaskCompletionSource<object> Tcs;
            public ToastView Toast;

            #endregion

            #region Constructors

            public ToastImpl()
            {
                Tcs = new TaskCompletionSource<object>();
            }

            #endregion

            #region Properties

            public Task CompletionTask => Tcs.Task;

            #endregion

            #region Methods

            public void Close()
            {
                Toast?.Hide();
            }

            #endregion
        }

        #endregion

        #region Fields

        private const string Key = "#currentToast";
        private readonly IThreadManager _threadManager;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public ToastPresenter([NotNull] IThreadManager threadManager)
        {
            Should.NotBeNull(threadManager, nameof(threadManager));
            _threadManager = threadManager;
        }

        #endregion

        #region Properties

        [CanBeNull]
        public static Func<object, UIView, float, ToastPosition, IDataContext, TaskCompletionSource<object>, ToastView> Factory { get; set; }

        #endregion

        #region Implementation of IToastPresenter

        public IToast ShowAsync(object content, float duration, ToastPosition position = ToastPosition.Bottom,
            IDataContext context = null)
        {
            var toast = new ToastImpl();
            if (_threadManager.IsUiThread)
                Show(content, duration, position, context, toast);
            else
                _threadManager.InvokeOnUiThreadAsync(() => Show(content, duration, position, context, toast));
            return toast;
        }

        #endregion

        #region Methods

        private void Show(object content, float duration, ToastPosition position, IDataContext context, ToastImpl toastImpl)
        {
            UIView owner = GetOwner() ?? UIApplication.SharedApplication.KeyWindow;
            toastImpl.Toast = CreateToast(content, owner, duration, position, context ?? DataContext.Empty, toastImpl.Tcs);
            ServiceProvider.AttachedValueProvider.AddOrUpdate(owner, Key, toastImpl.Toast,
                (item, value, currentValue, state) =>
                {
                    currentValue.Hide();
                    return value;
                }).Show();
        }

        protected virtual UIView GetOwner()
        {
            return UIApplication.SharedApplication.Windows.FirstOrDefault();
        }

        protected virtual ToastView CreateToast(object content, UIView owner, float duration, ToastPosition position,
            IDataContext context, TaskCompletionSource<object> tcs)
        {
            ToastView toastView = null;
#if !XAMARIN_FORMS
            var window = owner as UIWindow;
            if (window != null)
            {
                UIViewController controller = window.RootViewController;
                var navigationController = controller as MvvmNavigationController;
                if (navigationController != null)
                    controller = navigationController.TopViewController;
                var selector = controller?.GetBindingMemberValue(AttachedMembers.UIViewController.ToastTemplateSelector);
                if (selector != null)
                    toastView = (ToastView)selector.SelectTemplate(content, owner);
            }
#endif
            if (toastView == null)
            {
                var factory = Factory;
                if (factory != null)
                    toastView = factory(content, owner, duration, position, context, tcs);
            }
            if (toastView == null)
                toastView = new ToastView(content, owner);
            toastView.DisplayDuration = duration / 1000;
            toastView.Position = position;
            toastView.TaskCompletionSource = tcs;
            return toastView;
        }

        #endregion
    }
}
