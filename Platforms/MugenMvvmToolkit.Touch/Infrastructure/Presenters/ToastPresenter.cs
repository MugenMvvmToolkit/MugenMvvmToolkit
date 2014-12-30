#region Copyright

// ****************************************************************************
// <copyright file="ToastPresenter.cs">
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
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MonoTouch.UIKit;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Presenters;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Views;

namespace MugenMvvmToolkit.Infrastructure.Presenters
{
    public class ToastPresenter : IToastPresenter
    {
        #region Fields

        private const string Key = "#currentToast";
        private readonly IThreadManager _threadManager;

        #endregion

        #region Constructors

        public ToastPresenter([NotNull] IThreadManager threadManager)
        {
            Should.NotBeNull(threadManager, "threadManager");
            _threadManager = threadManager;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the factory to create toasts.
        /// </summary>
        [CanBeNull]
        public static Func<object, UIView, float, ToastPosition, IDataContext, TaskCompletionSource<object>, ToastView> Factory { get; set; }

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
                Show(content, duration, position, context, tcs);
            else
                _threadManager.InvokeOnUiThreadAsync(() => Show(content, duration, position, context, tcs));
            return tcs.Task;
        }

        #endregion

        #region Methods

        private void Show(object content, float duration, ToastPosition position, IDataContext context,
            TaskCompletionSource<object> tcs)
        {
            UIView owner = GetOwner() ?? UIApplication.SharedApplication.KeyWindow;
            ToastView toast = CreateToast(content, owner, duration, position, context ?? DataContext.Empty, tcs);
            ServiceProvider.AttachedValueProvider.AddOrUpdate(owner, Key, toast,
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
            var factory = Factory;
            if (factory != null)
                toastView = factory(content, owner, duration, position, context, tcs);
            return toastView ?? new ToastView(content, owner, duration, position, tcs);
        }

        #endregion
    }
}