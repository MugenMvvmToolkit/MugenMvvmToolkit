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
using Windows.Foundation;
using Windows.UI.Popups;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Presenters;
using MugenMvvmToolkit.Models;

#if XAMARIN_FORMS
#if WINDOWS_UWP
namespace MugenMvvmToolkit.Xamarin.Forms.UWP.Infrastructure.Presenters 
#else
namespace MugenMvvmToolkit.Xamarin.Forms.WinRT.Infrastructure.Presenters
#endif
#else
namespace MugenMvvmToolkit.UWP.Infrastructure.Presenters
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
            MessageResult defaultResult, TaskCompletionSource<MessageResult> tcs)
        {
            var messageDialog = new MessageDialog(messageBoxText, caption);
            switch (button)
            {
                case MessageButton.Ok:
                    messageDialog.Commands.Add(CreateUiCommand(MessageResult.Ok));
                    break;
                case MessageButton.OkCancel:
                    messageDialog.Commands.Add(CreateUiCommand(MessageResult.Ok));
                    messageDialog.Commands.Add(CreateUiCommand(MessageResult.Cancel));
                    break;
                case MessageButton.YesNo:
                    messageDialog.Commands.Add(CreateUiCommand(MessageResult.Yes));
                    messageDialog.Commands.Add(CreateUiCommand(MessageResult.No));
                    break;
                case MessageButton.YesNoCancel:
                    messageDialog.Commands.Add(CreateUiCommand(MessageResult.Yes));
                    messageDialog.Commands.Add(CreateUiCommand(MessageResult.No));
                    messageDialog.Commands.Add(CreateUiCommand(MessageResult.Cancel));
                    break;
                case MessageButton.AbortRetryIgnore:
                    messageDialog.Commands.Add(CreateUiCommand(MessageResult.Abort));
                    messageDialog.Commands.Add(CreateUiCommand(MessageResult.Retry));
                    messageDialog.Commands.Add(CreateUiCommand(MessageResult.Ignore));
                    break;
                case MessageButton.RetryCancel:
                    messageDialog.Commands.Add(CreateUiCommand(MessageResult.Retry));
                    messageDialog.Commands.Add(CreateUiCommand(MessageResult.Cancel));
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
            var result = messageDialog.ShowAsync();
            result.Completed = (info, status) =>
            {
                if (status == AsyncStatus.Canceled)
                {
                    tcs.TrySetCanceled();
                    return;
                }
                var command = info.GetResults();
                if (command == null || command.Id == null)
                    tcs.TrySetResult(MessageResult.None);
                else
                    tcs.TrySetResult((MessageResult)command.Id);
            };
        }

        private IUICommand CreateUiCommand(MessageResult result)
        {
            string text = GetButtonText(result);
            return new UICommand(text) { Id = result };
        }

        #endregion
    }
}
