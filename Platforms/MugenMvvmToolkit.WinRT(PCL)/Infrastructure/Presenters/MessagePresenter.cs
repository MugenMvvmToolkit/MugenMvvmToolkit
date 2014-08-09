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
using System.Threading.Tasks;
using Windows.UI.Popups;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Presenters;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Utils;

namespace MugenMvvmToolkit.Infrastructure.Presenters
{
    /// <summary>
    ///     Represents the base implementation of <see cref="IMessagePresenter" />.
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
                ShowMessage(messageBoxText, caption, button, defaultResult, tcs);
            else
                _threadManager.InvokeOnUiThreadAsync(() => ShowMessage(messageBoxText, caption, button, defaultResult, tcs));
            return tcs.Task;
        }

        #endregion

        #region Methods

        protected virtual string GetButtonText(MessageResult button)
        {
            return button.ToString();
        }

        private void ShowMessage(string messageBoxText, string caption, MessageButton button,
            MessageResult defaultResult,
            TaskCompletionSource<MessageResult> tcs)
        {
            var messageDialog = new MessageDialog(messageBoxText, caption);
            switch (button)
            {
                case MessageButton.Ok:
                    messageDialog.Commands.Add(CreateUiCommand(MessageResult.Ok, tcs));
                    break;
                case MessageButton.OkCancel:
                    messageDialog.Commands.Add(CreateUiCommand(MessageResult.Ok, tcs));
                    messageDialog.Commands.Add(CreateUiCommand(MessageResult.Cancel, tcs));
                    break;
                case MessageButton.YesNo:
                    messageDialog.Commands.Add(CreateUiCommand(MessageResult.Yes, tcs));
                    messageDialog.Commands.Add(CreateUiCommand(MessageResult.No, tcs));
                    break;
                case MessageButton.YesNoCancel:
                    messageDialog.Commands.Add(CreateUiCommand(MessageResult.Yes, tcs));
                    messageDialog.Commands.Add(CreateUiCommand(MessageResult.No, tcs));
                    if (ApplicationSettings.Platform.Platform != PlatformType.WinPhone)
                        messageDialog.Commands.Add(CreateUiCommand(MessageResult.Cancel, tcs));
                    break;
                case MessageButton.AbortRetryIgnore:
                    if (ApplicationSettings.Platform.Platform == PlatformType.WinPhone)
                        throw ExceptionManager.EnumOutOfRange("button", button);
                    messageDialog.Commands.Add(CreateUiCommand(MessageResult.Abort, tcs));
                    messageDialog.Commands.Add(CreateUiCommand(MessageResult.Retry, tcs));
                    messageDialog.Commands.Add(CreateUiCommand(MessageResult.Ignore, tcs));
                    break;
                case MessageButton.RetryCancel:
                    messageDialog.Commands.Add(CreateUiCommand(MessageResult.Retry, tcs));
                    messageDialog.Commands.Add(CreateUiCommand(MessageResult.Cancel, tcs));
                    break;
                default:
                    tcs.SetResult(MessageResult.None);
                    return;
            }
            for (int i = 0; i < messageDialog.Commands.Count; i++)
            {
                if (defaultResult == (MessageResult)messageDialog.Commands[i].Id)
                {
                    messageDialog.DefaultCommandIndex = (uint)i;
                    break;
                }
            }
            messageDialog.ShowAsync();
        }

        private IUICommand CreateUiCommand(MessageResult result, TaskCompletionSource<MessageResult> tcs)
        {
            string text = GetButtonText(result);
            return new UICommand(text, command => tcs.SetResult((MessageResult)command.Id)) { Id = result };
        }

        #endregion
    }
}