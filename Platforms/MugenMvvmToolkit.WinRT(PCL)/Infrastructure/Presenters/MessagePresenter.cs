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
using Windows.UI.Popups;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Presenters;
using MugenMvvmToolkit.Models;

#if XAMARIN_FORMS
using System;
namespace MugenMvvmToolkit.Xamarin.Forms.WinRT.Infrastructure.Presenters
#else
namespace MugenMvvmToolkit.WinRT.Infrastructure.Presenters
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
            Should.NotBeNull(threadManager, "threadManager");
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
                    if (MvvmApplication.Current.Platform.Platform != PlatformType.WinPhone)
                        messageDialog.Commands.Add(CreateUiCommand(MessageResult.Cancel, tcs));
                    break;
                case MessageButton.AbortRetryIgnore:
                    if (MvvmApplication.Current.Platform.Platform == PlatformType.WinPhone)
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
