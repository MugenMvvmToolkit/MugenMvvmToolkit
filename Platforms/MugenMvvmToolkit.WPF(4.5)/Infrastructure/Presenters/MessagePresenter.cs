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

using System.Threading.Tasks;
using System.Windows;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Presenters;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Infrastructure.Presenters
{
    /// <summary>
    ///     Represents the base implementation of <see cref="IMessagePresenter" />.
    /// </summary>
    public sealed class MessagePresenter : IMessagePresenter
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
            bool success;
            MessageBoxButton buttons = ConvertMessageBoxButtons(button, out success);
            Should.BeSupported(success, "The MessageBoxAdapter doesn't support {0} value", button);

            if (_threadManager.IsUiThread)
            {
                MessageBoxResult result = MessageBox.Show(messageBoxText, caption, buttons,
                    ConvertMessageBoxImages(icon),
                    ConvertMessageBoxResults(defaultResult));
                return ToolkitExtensions.FromResult(ConvertMessageBoxResult(result));
            }
            var tcs = new TaskCompletionSource<MessageResult>();
            _threadManager.InvokeOnUiThreadAsync(() =>
            {
                MessageBoxResult result = MessageBox.Show(messageBoxText, caption, buttons,
                    ConvertMessageBoxImages(icon), ConvertMessageBoxResults(defaultResult));
                tcs.SetResult(ConvertMessageBoxResult(result));
            });
            return tcs.Task;
        }

        #endregion

        #region Methods

        private static MessageResult ConvertMessageBoxResult(MessageBoxResult messageBox)
        {
            switch (messageBox)
            {
                case MessageBoxResult.OK:
                    return MessageResult.Ok;
                case MessageBoxResult.Cancel:
                    return MessageResult.Cancel;
                case MessageBoxResult.Yes:
                    return MessageResult.Yes;
                case MessageBoxResult.No:
                    return MessageResult.No;
                default:
                    return MessageResult.None;
            }
        }

        private static MessageBoxResult ConvertMessageBoxResults(MessageResult messageBox)
        {
            switch (messageBox)
            {
                case MessageResult.Ok:
                    return MessageBoxResult.OK;
                case MessageResult.Cancel:
                    return MessageBoxResult.Cancel;
                case MessageResult.No:
                    return MessageBoxResult.No;
                case MessageResult.Yes:
                    return MessageBoxResult.Yes;
                default:
                    return MessageBoxResult.None;
            }
        }

        private static MessageBoxButton ConvertMessageBoxButtons(MessageButton messageBoxButtons, out bool success)
        {
            success = true;
            switch (messageBoxButtons)
            {
                case MessageButton.Ok:
                    return MessageBoxButton.OK;
                case MessageButton.OkCancel:
                    return MessageBoxButton.OKCancel;
                case MessageButton.YesNo:
                    return MessageBoxButton.YesNo;
                case MessageButton.YesNoCancel:
                    return MessageBoxButton.YesNoCancel;
                default:
                    success = false;
                    return MessageBoxButton.OK;
            }
        }

        private static MessageBoxImage ConvertMessageBoxImages(MessageImage messageBoxImages)
        {
            switch (messageBoxImages)
            {
                case MessageImage.Asterisk:
                    return MessageBoxImage.Asterisk;
                case MessageImage.Error:
                    return MessageBoxImage.Error;
                case MessageImage.Exclamation:
                    return MessageBoxImage.Exclamation;
                case MessageImage.Hand:
                    return MessageBoxImage.Hand;
                case MessageImage.Information:
                    return MessageBoxImage.Information;
                case MessageImage.None:
                    return MessageBoxImage.None;
                case MessageImage.Question:
                    return MessageBoxImage.Question;
                case MessageImage.Stop:
                    return MessageBoxImage.Stop;
                case MessageImage.Warning:
                    return MessageBoxImage.Warning;
                default:
                    return MessageBoxImage.None;
            }
        }

        #endregion
    }
}