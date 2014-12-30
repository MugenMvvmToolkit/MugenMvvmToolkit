#region Copyright

// ****************************************************************************
// <copyright file="MessagePresenter.cs">
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
using System.Threading.Tasks;
using MonoTouch.UIKit;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Presenters;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Infrastructure.Presenters
{
    /// <summary>
    ///     Represent the base class for message box.
    /// </summary>
    public class MessagePresenter : IMessagePresenter
    {
        #region Fields

        private readonly IThreadManager _threadManager;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="MessagePresenter" /> class.
        /// </summary>
        public MessagePresenter(IThreadManager threadManager)
        {
            Should.NotBeNull(threadManager, "threadManager");
            _threadManager = threadManager;
        }

        #endregion

        #region Implementation of IMessagePresenter

        /// <summary>
        ///     Displays a message box that has a message, title bar caption, button, and icon; and that accepts a default message
        ///     box result and returns a result.
        /// </summary>
        /// <param name="message">A <see cref="T:System.String" /> that specifies the text to display.</param>
        /// <param name="caption">A <see cref="T:System.String" /> that specifies the title bar caption to display.</param>
        /// <param name="button">A <see cref="MessageButton" /> value that specifies which button or buttons to display.</param>
        /// <param name="icon">A <see cref="MessageImage" /> value that specifies the icon to display.</param>
        /// <param name="defaultResult">
        ///     A <see cref="MessageResult" /> value that specifies the default result of the message
        ///     box.
        /// </param>
        /// <param name="context">The specified context.</param>
        /// <returns>A <see cref="MessageResult" /> value that specifies which message box button is clicked by the user.</returns>
        public Task<MessageResult> ShowAsync(string message, string caption = "",
            MessageButton button = MessageButton.Ok, MessageImage icon = MessageImage.None,
            MessageResult defaultResult = MessageResult.None, IDataContext context = null)
        {
            var tcs = new TaskCompletionSource<MessageResult>();
            if (_threadManager.IsUiThread)
                ShowMessage(message, caption, button, tcs);
            else
                _threadManager.InvokeOnUiThreadAsync(() => ShowMessage(message, caption, button, tcs));
            return tcs.Task;
        }

        #endregion

        #region Methods

        protected virtual string GetButtonText(MessageResult button)
        {
            return button.ToString();
        }

        private void ShowMessage(string message, string caption, MessageButton button, TaskCompletionSource<MessageResult> tcs)
        {
            var alertView = new UIAlertView { Title = caption, Message = message };
            switch (button)
            {
                case MessageButton.Ok:
                    alertView.AddButton(GetButtonText(MessageResult.Ok));
                    break;
                case MessageButton.OkCancel:
                    alertView.AddButton(GetButtonText(MessageResult.Ok));
                    alertView.AddButton(GetButtonText(MessageResult.Cancel));
                    break;
                case MessageButton.YesNo:
                    alertView.AddButton(GetButtonText(MessageResult.Yes));
                    alertView.AddButton(GetButtonText(MessageResult.No));
                    break;
                case MessageButton.YesNoCancel:
                    alertView.AddButton(GetButtonText(MessageResult.Yes));
                    alertView.AddButton(GetButtonText(MessageResult.No));
                    alertView.AddButton(GetButtonText(MessageResult.Cancel));
                    break;
                case MessageButton.AbortRetryIgnore:
                    alertView.AddButton(GetButtonText(MessageResult.Abort));
                    alertView.AddButton(GetButtonText(MessageResult.Retry));
                    alertView.AddButton(GetButtonText(MessageResult.Ignore));
                    break;
                case MessageButton.RetryCancel:
                    alertView.AddButton(GetButtonText(MessageResult.Retry));
                    alertView.AddButton(GetButtonText(MessageResult.Cancel));
                    break;
                default:
                    throw new ArgumentOutOfRangeException("button");
            }
            EventHandler<UIButtonEventArgs> handler = null;
            handler = (sender, args) =>
            {
                alertView.Clicked -= handler;
                switch (button)
                {
                    case MessageButton.Ok:
                        tcs.SetResult(MessageResult.Ok);
                        break;
                    case MessageButton.OkCancel:
                        tcs.SetResult(args.ButtonIndex == 0 ? MessageResult.Ok : MessageResult.Cancel);
                        break;
                    case MessageButton.YesNo:
                        tcs.SetResult(args.ButtonIndex == 0 ? MessageResult.Yes : MessageResult.No);
                        break;
                    case MessageButton.YesNoCancel:
                        switch (args.ButtonIndex)
                        {
                            case 0:
                                tcs.SetResult(MessageResult.Yes);
                                break;
                            case 1:
                                tcs.SetResult(MessageResult.No);
                                break;
                            case 2:
                                tcs.SetResult(MessageResult.Cancel);
                                break;
                        }
                        break;
                    case MessageButton.AbortRetryIgnore:
                        switch (args.ButtonIndex)
                        {
                            case 0:
                                tcs.SetResult(MessageResult.Abort);
                                break;
                            case 1:
                                tcs.SetResult(MessageResult.Retry);
                                break;
                            case 2:
                                tcs.SetResult(MessageResult.Ignore);
                                break;
                        }
                        break;
                    case MessageButton.RetryCancel:
                        tcs.SetResult(args.ButtonIndex == 0 ? MessageResult.Retry : MessageResult.Cancel);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("button");
                }
            };
            alertView.Clicked += handler;
            alertView.Show();
        }

        #endregion
    }
}