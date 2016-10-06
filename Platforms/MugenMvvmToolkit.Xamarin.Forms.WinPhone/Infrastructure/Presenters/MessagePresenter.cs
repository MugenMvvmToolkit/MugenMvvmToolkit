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

using System.Threading.Tasks;
using System.Windows;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Presenters;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Xamarin.Forms.WinPhone.Infrastructure.Presenters
{
    public sealed class MessagePresenter : IMessagePresenter
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
            bool success;
            MessageBoxButton buttons = ConvertMessageBoxButtons(button, out success);
            Should.BeSupported(success, "The MessageBoxAdapter doesn't support {0} value", button);

            if (_threadManager.IsUiThread)
            {
                MessageBoxResult result = MessageBox.Show(messageBoxText, caption, buttons);
                return ToolkitExtensions.FromResult(ConvertMessageBoxResult(result, button));
            }
            var tcs = new TaskCompletionSource<MessageResult>();
            _threadManager.InvokeOnUiThreadAsync(() =>
            {
                MessageBoxResult result = MessageBox.Show(messageBoxText, caption, buttons);
                tcs.SetResult(ConvertMessageBoxResult(result, button));
            });
            return tcs.Task;
        }

        #endregion

        #region Methods

        private static MessageResult ConvertMessageBoxResult(MessageBoxResult messageBox,
            MessageButton buttons)
        {
            switch (messageBox)
            {
                case MessageBoxResult.OK:
                    if (buttons == MessageButton.YesNo ||
                        buttons == MessageButton.YesNoCancel)
                        return MessageResult.Yes;
                    return MessageResult.Ok;
                case MessageBoxResult.Cancel:
                    if (buttons == MessageButton.YesNo ||
                        buttons == MessageButton.YesNoCancel)
                        return MessageResult.No;
                    return MessageResult.Cancel;
                case MessageBoxResult.Yes:
                    return MessageResult.Yes;
                case MessageBoxResult.No:
                    return MessageResult.No;
                default:
                    return MessageResult.None;
            }
        }

        private static MessageBoxButton ConvertMessageBoxButtons(MessageButton buttons, out bool success)
        {
            success = true;
            switch (buttons)
            {
                case MessageButton.Ok:
                    return MessageBoxButton.OK;
                case MessageButton.OkCancel:
                case MessageButton.YesNo:
                case MessageButton.YesNoCancel:
                    return MessageBoxButton.OKCancel;
                default:
                    success = false;
                    return MessageBoxButton.OK;
            }
        }

        #endregion
    }
}
