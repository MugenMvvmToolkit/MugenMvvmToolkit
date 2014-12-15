#region Copyright
// ****************************************************************************
// <copyright file="MessagePresenter.cs">
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
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Navigation;
using MugenMvvmToolkit.Interfaces.Presenters;
using MugenMvvmToolkit.Interfaces.Views;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Infrastructure.Presenters
{
    /// <summary>
    ///     Represent the base class for message box.
    /// </summary>
    public class MessagePresenter : IMessagePresenter
    {
        #region Fields

#if !XAMARIN_FORMS
        private readonly INavigationProvider _navigationProvider;
#endif
        private readonly IThreadManager _threadManager;

        #endregion

        #region Constructors

#if XAMARIN_FORMS
        /// <summary>
        ///     Initializes a new instance of the <see cref="MessagePresenter" /> class.
        /// </summary>
        public MessagePresenter(IThreadManager threadManager)
        {
            Should.NotBeNull(threadManager, "threadManager");
            _threadManager = threadManager;
        }
#else
        /// <summary>
        ///     Initializes a new instance of the <see cref="MessagePresenter" /> class.
        /// </summary>
        public MessagePresenter(INavigationProvider navigationProvider, IThreadManager threadManager)
        {
            Should.NotBeNull(navigationProvider, "navigationProvider");
            Should.NotBeNull(threadManager, "threadManager");
            _navigationProvider = navigationProvider;
            _threadManager = threadManager;
        }
#endif


        #endregion

        #region Implementation of IMessagePresenter

        /// <summary>
        ///     Displays a message box that has a message, title bar caption, button, and icon; and that accepts a default message
        ///     box result and returns a result.
        /// </summary>
        /// <param name="messageBoxText">A <see cref="T:System.String" /> that specifies the text to display.</param>
        /// <param name="caption">A <see cref="T:System.String" /> that specifies the title bar caption to display.</param>
        /// <param name="button">A <see cref="MessageButton" /> value that specifies which button or buttons to display.</param>
        /// <param name="icon">A <see cref="MessageImage" /> value that specifies the icon to display.</param>
        /// <param name="defaultResult">
        ///     A <see cref="MessageResult" /> value that specifies the default result of the message
        ///     box.
        /// </param>
        /// <param name="context">The specified context.</param>
        /// <returns>A <see cref="MessageResult" /> value that specifies which message box button is clicked by the user.</returns>
        public Task<MessageResult> ShowAsync(string messageBoxText, string caption = "",
            MessageButton button = MessageButton.Ok, MessageImage icon = MessageImage.None,
            MessageResult defaultResult = MessageResult.None, IDataContext context = null)
        {
            var tcs = new TaskCompletionSource<MessageResult>();
            if (_threadManager.IsUiThread)
                ShowMessage(messageBoxText, caption, button, icon, defaultResult, tcs);
            else
                _threadManager.InvokeOnUiThreadAsync(
                    () => ShowMessage(messageBoxText, caption, button, icon, defaultResult, tcs));
            return tcs.Task;
        }

        #endregion

        #region Methods

        protected virtual string GetButtonText(MessageResult button)
        {
            return button.ToString();
        }

        protected virtual int? GetIconResource(MessageImage icon)
        {
            return null;
        }

        private void ShowMessage(string messageBoxText, string caption, MessageButton button, MessageImage icon,
            MessageResult defaultResult,
            TaskCompletionSource<MessageResult> tcs)
        {
#if XAMARIN_FORMS
            var activity = Xamarin.Forms.Forms.Context;
#else
            var activity = _navigationProvider.CurrentContent as IActivityView;
#endif
            Should.BeSupported(activity != null, "The current top activity is null.");
            AlertDialog.Builder builder = new AlertDialog.Builder((Context)activity)
                .SetTitle(caption)
                .SetMessage(messageBoxText)
                .SetCancelable(false);
            switch (button)
            {
                case MessageButton.Ok:
                    builder.SetPositiveButton(GetButtonText(MessageResult.Ok),
                        (sender, args) => tcs.TrySetResult(MessageResult.Ok));
                    break;
                case MessageButton.OkCancel:
                    builder.SetPositiveButton(GetButtonText(MessageResult.Ok),
                        (sender, args) => tcs.TrySetResult(MessageResult.Ok));
                    builder.SetNegativeButton(GetButtonText(MessageResult.Cancel),
                        (sender, args) => tcs.TrySetResult(MessageResult.Cancel));
                    break;
                case MessageButton.YesNo:
                    builder.SetPositiveButton(GetButtonText(MessageResult.Yes),
                        (sender, args) => tcs.TrySetResult(MessageResult.Yes));
                    builder.SetNegativeButton(GetButtonText(MessageResult.No),
                        (sender, args) => tcs.TrySetResult(MessageResult.No));
                    break;
                case MessageButton.YesNoCancel:
                    builder.SetPositiveButton(GetButtonText(MessageResult.Yes),
                        (sender, args) => tcs.TrySetResult(MessageResult.Yes));
                    builder.SetNeutralButton(GetButtonText(MessageResult.No),
                        (sender, args) => tcs.TrySetResult(MessageResult.No));
                    builder.SetNegativeButton(GetButtonText(MessageResult.Cancel),
                        (sender, args) => tcs.TrySetResult(MessageResult.Cancel));
                    break;
                case MessageButton.AbortRetryIgnore:
                    builder.SetPositiveButton(GetButtonText(MessageResult.Abort),
                        (sender, args) => tcs.TrySetResult(MessageResult.Abort));
                    builder.SetNeutralButton(GetButtonText(MessageResult.Retry),
                        (sender, args) => tcs.TrySetResult(MessageResult.Retry));
                    builder.SetNegativeButton(GetButtonText(MessageResult.Ignore),
                        (sender, args) => tcs.TrySetResult(MessageResult.Ignore));
                    break;
                case MessageButton.RetryCancel:
                    builder.SetPositiveButton(GetButtonText(MessageResult.Retry),
                        (sender, args) => tcs.TrySetResult(MessageResult.Retry));
                    builder.SetNeutralButton(GetButtonText(MessageResult.Cancel),
                        (sender, args) => tcs.TrySetResult(MessageResult.Cancel));
                    break;
                default:
                    throw new ArgumentOutOfRangeException("button");
            }
            int? drawable = GetIconResource(icon);
            if (drawable != null)
                builder.SetIcon(drawable.Value);
            AlertDialog dialog = builder.Create();
#if !XAMARIN_FORMS
            activity.Mediator.Destroyed += (sender, args) => tcs.TrySetResult(defaultResult);
#endif
            dialog.Show();
        }

        #endregion
    }
}