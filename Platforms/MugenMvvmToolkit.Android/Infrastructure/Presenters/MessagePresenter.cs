#region Copyright

// ****************************************************************************
// <copyright file="MessagePresenter.cs">
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
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Presenters;
using MugenMvvmToolkit.Models;

#if XAMARIN_FORMS && ANDROID
namespace MugenMvvmToolkit.Xamarin.Forms.Android.Infrastructure.Presenters
#elif ANDROID
using MugenMvvmToolkit.Android.Interfaces.Views;

namespace MugenMvvmToolkit.Android.Infrastructure.Presenters
#endif
{
    public class MessagePresenter : IMessagePresenter
    {
        #region Fields

        private readonly IThreadManager _threadManager;

        #endregion

        #region Constructors

        public MessagePresenter(IThreadManager threadManager)
        {
            Should.NotBeNull(threadManager, nameof(threadManager));
            _threadManager = threadManager;
        }

        #endregion

        #region Implementation of IMessagePresenter

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
            var activity = global::Xamarin.Forms.Forms.Context;
#else
            var activity = PlatformExtensions.CurrentActivity as IActivityView;
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
                    if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
                    {
                        builder.SetNegativeButton(GetButtonText(MessageResult.No),
                            (sender, args) => tcs.TrySetResult(MessageResult.No));
                        builder.SetNeutralButton(GetButtonText(MessageResult.Cancel),
                            (sender, args) => tcs.TrySetResult(MessageResult.Cancel));
                    }
                    else
                    {
                        builder.SetNeutralButton(GetButtonText(MessageResult.No),
                            (sender, args) => tcs.TrySetResult(MessageResult.No));
                        builder.SetNegativeButton(GetButtonText(MessageResult.Cancel),
                            (sender, args) => tcs.TrySetResult(MessageResult.Cancel));
                    }
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
                    builder.SetNegativeButton(GetButtonText(MessageResult.Cancel),
                        (sender, args) => tcs.TrySetResult(MessageResult.Cancel));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(button));
            }
            int? drawable = GetIconResource(icon);
            if (drawable != null)
                builder.SetIcon(drawable.Value);
            AlertDialog dialog = builder.Create();
#if !XAMARIN_FORMS
            EventHandler<Activity, EventArgs> handler = null;
            handler = (sender, args) =>
            {
                ((IActivityView)sender).Mediator.Destroyed -= handler;
                tcs.TrySetResult(defaultResult);
            };
            activity.Mediator.Destroyed += handler;
#endif
            dialog.Show();
        }

        #endregion
    }
}
