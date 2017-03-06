#region Copyright

// ****************************************************************************
// <copyright file="RelayCommandTest.cs">
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
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.Messages;
using MugenMvvmToolkit.Test.TestModels;
using Should;

namespace MugenMvvmToolkit.Test.Models
{
    [TestClass]
    public class RelayCommandTest : TestBase
    {
        #region Fields

        protected static readonly Action<object> NodoAction = o => { };

        #endregion

        #region Methods

        [TestMethod]
        public void CmdShouldUseDelegates()
        {
            bool executeInvoked = false;
            bool canExecuteInvoked = false;
            var parameter = new object();
            var cmd = CreateCommand(o =>
            {
                o.ShouldEqual(parameter);
                executeInvoked = true;
            }, o =>
            {
                o.ShouldEqual(parameter);
                canExecuteInvoked = true;
                return true;
            });
            cmd.ExecutionMode = CommandExecutionMode.None;
            cmd.CanExecuteMode = ExecutionMode.None;
            cmd.Execute(parameter);
            cmd.CanExecute(parameter);
            executeInvoked.ShouldBeTrue();
            canExecuteInvoked.ShouldBeTrue();
        }

        [TestMethod]
        public void CmdShouldUseGlobalSettingsToGetDefaultExecutionMode()
        {
            ApplicationSettings.CommandExecutionMode = CommandExecutionMode.None;
            RelayCommandBase relayCommand = CreateCommand(NodoAction);
            relayCommand.ExecutionMode.ShouldEqual(CommandExecutionMode.None);

            ApplicationSettings.CommandExecutionMode = CommandExecutionMode.CanExecuteBeforeExecute;
            relayCommand = CreateCommand(NodoAction);
            relayCommand.ExecutionMode.ShouldEqual(CommandExecutionMode.CanExecuteBeforeExecute);
        }

        [TestMethod]
        public void CmdShouldUseGlobalSettingsToGetDefaultCanExecuteMode()
        {
            ApplicationSettings.CommandCanExecuteMode = ExecutionMode.None;
            var relayCommand = CreateCommand(NodoAction);
            relayCommand.CanExecuteMode.ShouldEqual(ExecutionMode.None);

            ApplicationSettings.CommandCanExecuteMode = ExecutionMode.AsynchronousOnUiThread;
            relayCommand = CreateCommand(NodoAction);
            relayCommand.CanExecuteMode.ShouldEqual(ExecutionMode.AsynchronousOnUiThread);
        }

        [TestMethod]
        public void CmdShouldNotAddNotifierWhenCanExecuteNull()
        {
            var notifier = new BindingSourceModel();
            var relayCommand = CreateCommand(NodoAction);
            relayCommand.AddNotifier(notifier).ShouldBeFalse();
            relayCommand.GetNotifiers().ShouldBeEmpty();
        }

        [TestMethod]
        public void CmdShouldNotAddNotifierConstructorWhenCanExecuteNull()
        {
            var notifier = new BindingSourceModel();
            var relayCommand = CreateCommand(NodoAction, null, notifier);
            relayCommand.GetNotifiers().ShouldBeEmpty();
        }

        [TestMethod]
        public void CmdShouldAddNotifierWhenCanExecuteNotNull()
        {
            var notifier = new BindingSourceModel();
            var relayCommand = CreateCommand(NodoAction, o => true);
            relayCommand.AddNotifier(notifier).ShouldBeTrue();
            relayCommand.GetNotifiers().Single().ShouldEqual(notifier);
        }

        [TestMethod]
        public void CmdShouldAddNotifierConstructorWhenCanExecuteNotNull()
        {
            var notifier = new BindingSourceModel();
            var relayCommand = CreateCommand(NodoAction, o => true, notifier);
            relayCommand.GetNotifiers().Single().ShouldEqual(notifier);
        }

        [TestMethod]
        public void CmdShouldRemoveNotifier()
        {
            var notifier = new BindingSourceModel();
            var relayCommand = CreateCommand(NodoAction, o => true);
            relayCommand.AddNotifier(notifier).ShouldBeTrue();
            relayCommand.GetNotifiers().Single().ShouldEqual(notifier);

            relayCommand.RemoveNotifier(notifier).ShouldBeTrue();
            relayCommand.GetNotifiers().ShouldBeEmpty();
        }

        [TestMethod]
        public void CmdShouldClearAllNotifiers()
        {
            var notifier1 = new BindingSourceModel();
            var notifier2 = new BindingSourceModel();
            var relayCommand = CreateCommand(NodoAction, o => true);
            relayCommand.AddNotifier(notifier1).ShouldBeTrue();
            relayCommand.AddNotifier(notifier2).ShouldBeTrue();
            var list = relayCommand.GetNotifiers();
            list.Count.ShouldEqual(2);
            list.Contains(notifier1).ShouldBeTrue();
            list.Contains(notifier2).ShouldBeTrue();

            relayCommand.ClearNotifiers();
            relayCommand.GetNotifiers().ShouldBeEmpty();
        }

        [TestMethod]
        public void CmdShouldClearAllNotifiersOnDispose()
        {
            var notifier1 = new BindingSourceModel();
            var notifier2 = new BindingSourceModel();
            var relayCommand = CreateCommand(NodoAction, o => true);
            relayCommand.AddNotifier(notifier1).ShouldBeTrue();
            relayCommand.AddNotifier(notifier2).ShouldBeTrue();
            var list = relayCommand.GetNotifiers();
            list.Count.ShouldEqual(2);
            list.Contains(notifier1).ShouldBeTrue();
            list.Contains(notifier2).ShouldBeTrue();

            relayCommand.Dispose();
            relayCommand.GetNotifiers().ShouldBeEmpty();
        }

        [TestMethod]
        public void CmdShouldNotAddNotifierNotSupportedNotifier()
        {
            var notifier = new object();
            var relayCommand = CreateCommand(NodoAction, o => true);
            relayCommand.AddNotifier(notifier).ShouldBeFalse();
            relayCommand.GetNotifiers().ShouldBeEmpty();
        }

        [TestMethod]
        public void CmdShouldUseExecuteModeNone()
        {
            const bool canExecute = false;
            bool isInvoked = false;
            Action<object> action = o => isInvoked = true;
            var relayCommand = CreateCommand(action, o => canExecute);

            relayCommand.ExecutionMode = CommandExecutionMode.None;
            relayCommand.Execute(null);
            isInvoked.ShouldBeTrue();
        }

        [TestMethod]
        public void CmdShouldUseExecuteModeCanExecuteBeforeExecute()
        {
            bool canExecute = false;
            bool isInvoked = false;
            Action<object> action = o => isInvoked = true;
            var relayCommand = CreateCommand(action, o => canExecute);

            relayCommand.ExecutionMode = CommandExecutionMode.CanExecuteBeforeExecute;
            relayCommand.Execute(null);
            isInvoked.ShouldBeFalse();

            canExecute = true;
            relayCommand.Execute(null);
            isInvoked.ShouldBeTrue();
        }

        [TestMethod]
        public void CmdShouldUseExecuteModeCanExecuteBeforeExecuteWithException()
        {
            bool canExecute = false;
            bool isInvoked = false;
            Action<object> action = o => isInvoked = true;
            var relayCommand = CreateCommand(action, o => canExecute);

            relayCommand.ExecutionMode = CommandExecutionMode.CanExecuteBeforeExecuteWithException;
            ShouldThrow<InvalidOperationException>(() => relayCommand.Execute(null));
            isInvoked.ShouldBeFalse();

            canExecute = true;
            relayCommand.Execute(null);
            isInvoked.ShouldBeTrue();
        }

        [TestMethod]
        public void CanExecuteShouldReturnValueFromDelegate()
        {
            bool canExecute = false;
            var relayCommand = CreateCommand(NodoAction, o => canExecute);
            relayCommand.CanExecute(null).ShouldBeFalse();
            canExecute = true;
            relayCommand.CanExecute(null).ShouldBeTrue();
        }

        [TestMethod]
        public void RaiseCanExecuteChangedShouldUseCanExecuteModeNone()
        {
            bool isInvoked = false;
            var relayCommand = CreateCommand(NodoAction, o => true);
            relayCommand.CanExecuteMode = ExecutionMode.None;
            relayCommand.CanExecuteChanged += (sender, args) =>
            {
                isInvoked = true;
            };
            relayCommand.RaiseCanExecuteChanged();
            isInvoked.ShouldBeTrue();
        }

        [TestMethod]
        public void RaiseCanExecuteChangedShouldUseCanExecuteModeAsynchronousOnUiThread()
        {
            bool isInvoked = false;
            var relayCommand = CreateCommand(NodoAction, o => true);
            relayCommand.CanExecuteMode = ExecutionMode.AsynchronousOnUiThread;
            relayCommand.CanExecuteChanged += (sender, args) =>
            {
                isInvoked = true;
            };
            relayCommand.RaiseCanExecuteChanged();
            isInvoked.ShouldBeFalse();
            ThreadManager.InvokeOnUiThreadAsync();
            isInvoked.ShouldBeTrue();
        }

        [TestMethod]
        public void RaiseCanExecuteChangedShouldUseCanExecuteModeSynchronousOnUiThread()
        {
            bool isInvoked = false;
            var relayCommand = CreateCommand(NodoAction, o => true);
            relayCommand.CanExecuteMode = ExecutionMode.SynchronousOnUiThread;
            relayCommand.CanExecuteChanged += (sender, args) =>
            {
                isInvoked = true;
            };
            relayCommand.RaiseCanExecuteChanged();
            isInvoked.ShouldBeFalse();
            ThreadManager.InvokeOnUiThread();
            isInvoked.ShouldBeTrue();
        }

        [TestMethod]
        public void RaiseCanExecuteChangedShouldBeInvokedOnPropertyChanged()
        {
            var notifier = new BindingSourceModel();
            bool isInvoked = false;
            var relayCommand = CreateCommand(NodoAction, o => true);
            relayCommand.CanExecuteMode = ExecutionMode.None;
            relayCommand.CanExecuteChanged += (sender, args) =>
            {
                isInvoked = true;
            };
            relayCommand.AddNotifier(notifier);
            notifier.OnPropertyChanged("Test", ExecutionMode.None);
            isInvoked.ShouldBeTrue();
        }

        [TestMethod]
        public void RaiseCanExecuteChangedShouldBeInvokedOnStateChangedMessage()
        {
            var notifier = new EventAggregator(true);
            bool isInvoked = false;
            var relayCommand = CreateCommand(NodoAction, o => true);
            relayCommand.CanExecuteMode = ExecutionMode.None;
            relayCommand.CanExecuteChanged += (sender, args) =>
            {
                isInvoked = true;
            };
            relayCommand.AddNotifier(notifier);
            notifier.Publish(notifier, StateChangedMessage.Empty);
            isInvoked.ShouldBeTrue();
        }

        [TestMethod]
        public void RaiseCanExecuteChangedShouldSuspendNotification()
        {
            bool isInvoked = false;
            var relayCommand = CreateCommand(NodoAction, o => true);
            relayCommand.CanExecuteMode = ExecutionMode.None;
            relayCommand.CanExecuteChanged += (sender, args) =>
            {
                isInvoked = true;
            };
            using (relayCommand.SuspendNotifications())
            {
                relayCommand.RaiseCanExecuteChanged();
                using (relayCommand.SuspendNotifications())
                {
                    relayCommand.RaiseCanExecuteChanged();
                }
                isInvoked.ShouldBeFalse();
            }
            isInvoked.ShouldBeTrue();
        }

        [TestMethod]
        public void CmdShouldUseGlobalHandlers()
        {
            bool addInvoked = false;
            bool removeInvoked = false;
            EventHandler handler = (sender, args) => { };
            var relayCommand = CreateCommand(NodoAction, o => true);
            ApplicationSettings.CommandAddCanExecuteChangedEvent = (@base, eventHandler) =>
            {
                @base.ShouldEqual(relayCommand);
                addInvoked = true;
                eventHandler.ShouldEqual(handler);
            };
            ApplicationSettings.CommandRemoveCanExecuteChangedEvent = (@base, eventHandler) =>
            {
                @base.ShouldEqual(relayCommand);
                removeInvoked = true;
                eventHandler.ShouldEqual(handler);
            };

            relayCommand.CanExecuteChanged += handler;
            relayCommand.CanExecuteChanged -= handler;
            addInvoked.ShouldBeTrue();
            removeInvoked.ShouldBeTrue();

            ApplicationSettings.CommandAddCanExecuteChangedEvent = null;
            ApplicationSettings.CommandRemoveCanExecuteChangedEvent = null;
        }

        [TestMethod]
        public void CmdShouldBeExecutedInNewThreadExecuteAsynchronouslyTrue()
        {
            bool isInvoked = false;
            var cmd = CreateCommand(o => isInvoked = true, o => true);
            cmd.ExecuteAsynchronously = true;
            ThreadManager.ImmediateInvokeAsync = false;
            cmd.CanExecuteMode = ExecutionMode.None;

            cmd.Execute(null);
            isInvoked.ShouldBeFalse();
            ThreadManager.InvokeAsync();
            isInvoked.ShouldBeTrue();
        }

        [TestMethod]
        public virtual void TaskCmdShouldNotBeExecuteMultipleTimes()
        {
            bool isInvoked = false;
            var tcs = new TaskCompletionSource<object>();
            var command = RelayCommandBase.FromAsyncHandler(() =>
            {
                isInvoked = true;
                return tcs.Task;
            }, null, false);

            command.Execute(null);
            isInvoked.ShouldBeTrue();

            isInvoked = false;
            command.Execute(null);
            isInvoked.ShouldBeFalse();

            isInvoked = false;
            tcs.SetResult(null);
            command.Execute(null);
            isInvoked.ShouldBeTrue();
        }

        [TestMethod]
        public virtual void GenericTaskCmdShouldNotBeExecuteMultipleTimes()
        {
            bool isInvoked = false;
            var tcs = new TaskCompletionSource<object>();
            var command = RelayCommandBase.FromAsyncHandler<object>(o =>
            {
                o.ShouldEqual(tcs);
                isInvoked = true;
                return tcs.Task;
            }, null, false);

            command.Execute(tcs);
            isInvoked.ShouldBeTrue();

            isInvoked = false;
            command.Execute(tcs);
            isInvoked.ShouldBeFalse();

            isInvoked = false;
            tcs.SetResult(tcs);
            command.Execute(tcs);
            isInvoked.ShouldBeTrue();
        }

        protected virtual RelayCommandBase CreateCommand(Action<object> execute, Func<object, bool> canExecute = null,
            params object[] items)
        {
            return new RelayCommand(execute, canExecute, items);
        }

        #endregion
    }
}
