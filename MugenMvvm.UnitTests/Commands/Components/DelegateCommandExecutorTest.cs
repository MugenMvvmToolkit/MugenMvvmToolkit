#pragma warning disable 4014
using System;
using System.Threading.Tasks;
using MugenMvvm.Commands;
using MugenMvvm.Commands.Components;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.UnitTests.Commands.Internal;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Commands.Components
{
    public class DelegateCommandExecutorTest : UnitTestBase
    {
        private readonly CompositeCommand _command;

        public DelegateCommandExecutorTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _command = new CompositeCommand(null, ComponentCollectionManager);
        }

        [Fact]
        public void ShouldNotThrowException()
        {
            var exception = new NotSupportedException();
            Action execute = () => throw exception;
            var component = new DelegateCommandExecutor<object>(execute, null, CommandExecutionBehavior.None, true);
            var task = component.ExecuteAsync(_command, null, default, null).AsTask();
            task.IsFaulted.ShouldBeTrue();
            task.Exception!.GetBaseException().ShouldEqual(exception);
        }

        [Fact]
        public async Task ShouldSupportAction()
        {
            var executed = 0;
            Action execute = () => ++executed;
            var component = new DelegateCommandExecutor<object>(execute, null, CommandExecutionBehavior.None, true);
            await component.ExecuteAsync(_command, null, default, null);
            executed.ShouldEqual(1);
        }

        [Fact]
        public async Task ShouldSupportActionWithObject()
        {
            var executed = 0;
            Action<object> execute = item =>
            {
                item.ShouldEqual(this);
                ++executed;
            };
            var component = new DelegateCommandExecutor<object>(execute, null, CommandExecutionBehavior.None, true);
            await component.ExecuteAsync(_command, this, default, null);
            executed.ShouldEqual(1);
        }

        [Fact]
        public void ShouldSupportCanExecuteFunc()
        {
            var executed = 0;
            var canExecuteValue = false;
            Action execute = () => { };
            Func<bool> canExecute = () =>
            {
                ++executed;
                return canExecuteValue;
            };
            var component = new DelegateCommandExecutor<object>(execute, canExecute, CommandExecutionBehavior.None, true);
            component.CanExecute(_command, null, null).ShouldEqual(canExecuteValue);
            executed.ShouldEqual(1);

            canExecuteValue = true;
            component.CanExecute(_command, null, null).ShouldEqual(canExecuteValue);
            executed.ShouldEqual(2);
        }

        [Fact]
        public void ShouldSupportCanExecuteFuncWithObject()
        {
            var executed = 0;
            var canExecuteValue = false;
            Action execute = () => { };
            Func<object?, bool> canExecute = item =>
            {
                item.ShouldEqual(this);
                ++executed;
                return canExecuteValue;
            };
            var component = new DelegateCommandExecutor<object>(execute, canExecute, CommandExecutionBehavior.None, true);
            component.CanExecute(_command, this, null).ShouldEqual(canExecuteValue);
            executed.ShouldEqual(1);

            canExecuteValue = true;
            component.CanExecute(_command, this, null).ShouldEqual(canExecuteValue);
            executed.ShouldEqual(2);
        }

        [Fact]
        public async Task ShouldSupportCommandExecutionModeCanExecuteBeforeExecute()
        {
            var executed = 0;
            var canExecuted = 0;
            Action execute = () => ++executed;
            var canExecuteValue = false;
            Func<bool> canExecute = () =>
            {
                ++canExecuted;
                return canExecuteValue;
            };

            var component = new DelegateCommandExecutor<object>(execute, canExecute, CommandExecutionBehavior.CheckCanExecute, true);
            _command.AddComponent(component);

            await component.ExecuteAsync(_command, null, default, null);
            executed.ShouldEqual(0);
            canExecuted.ShouldEqual(1);

            canExecuteValue = true;
            await component.ExecuteAsync(_command, null, default, null);
            executed.ShouldEqual(1);
            canExecuted.ShouldEqual(2);
        }

        [Fact]
        public async Task ShouldSupportCommandExecutionModeCanExecuteBeforeExecuteException()
        {
            var executed = 0;
            var canExecuted = 0;
            Action execute = () => ++executed;
            var canExecuteValue = false;
            Func<bool> canExecute = () =>
            {
                ++canExecuted;
                return canExecuteValue;
            };

            var component = new DelegateCommandExecutor<object>(execute, canExecute, CommandExecutionBehavior.CheckCanExecuteThrow, true);
            _command.AddComponent(component);

            var task = component.ExecuteAsync(_command, null, default, null).AsTask();
            task.IsFaulted.ShouldBeTrue();
            task.Exception!.GetBaseException().ShouldBeType<InvalidOperationException>();
            executed.ShouldEqual(0);
            canExecuted.ShouldEqual(1);

            canExecuteValue = true;
            task = component.ExecuteAsync(_command, null, default, null).AsTask();
            (await task).ShouldBeTrue();
            executed.ShouldEqual(1);
            canExecuted.ShouldEqual(2);
        }

        [Fact]
        public async Task ShouldSupportCommandExecutionModeNone()
        {
            var executed = 0;
            var canExecuted = 0;
            Action execute = () => ++executed;
            var canExecuteValue = false;
            Func<bool> canExecute = () =>
            {
                ++canExecuted;
                return canExecuteValue;
            };

            var component = new DelegateCommandExecutor<object>(execute, canExecute, CommandExecutionBehavior.None, true);
            _command.AddComponent(component);

            await component.ExecuteAsync(_command, null, default, null);
            await component.ExecuteAsync(_command, null, default, null);
            executed.ShouldEqual(2);
            canExecuted.ShouldEqual(0);
        }

        [Fact]
        public async Task ShouldSupportFuncTask()
        {
            var executed = 0;
            var tcs = new TaskCompletionSource<object>();
            Func<Task> execute = () =>
            {
                ++executed;
                return tcs.Task;
            };
            var component = new DelegateCommandExecutor<object>(execute, null, CommandExecutionBehavior.None, true);
            var task = component.ExecuteAsync(_command, null, default, null)!;
            executed.ShouldEqual(1);
            task.IsCompleted.ShouldBeFalse();
            tcs.SetResult(this);
            await task;
            task.IsCompleted.ShouldBeTrue();
        }

        [Fact]
        public async Task ShouldSupportFuncTaskWithObject()
        {
            var executed = 0;
            var tcs = new TaskCompletionSource<object>();
            Func<object?, Task> execute = item =>
            {
                item.ShouldEqual(this);
                ++executed;
                return tcs.Task;
            };
            var component = new DelegateCommandExecutor<object>(execute, null, CommandExecutionBehavior.None, true);
            var task = component.ExecuteAsync(_command, this, default, null)!;
            executed.ShouldEqual(1);
            task.IsCompleted.ShouldBeFalse();
            tcs.SetResult(this);
            await task;
            task.IsCompleted.ShouldBeTrue();
        }

        [Theory]
        [InlineData(true, true, true)]
        [InlineData(true, true, false)]
        [InlineData(false, false, true)]
        [InlineData(true, false, false)]
        public void HasCanExecuteShouldBe(bool value, bool hasCanExecute, bool allowMultiply)
        {
            Action execute = () => { };
            Func<bool> canExecute = () => true;
            var component = new DelegateCommandExecutor<object>(execute, hasCanExecute ? canExecute : null, CommandExecutionBehavior.None, allowMultiply);
            component.HasCanExecute(_command, null).ShouldEqual(value);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ShouldSupportAllowMultipleExecution(bool value)
        {
            var executed = 0;
            var tcs = new TaskCompletionSource<object>();
            Func<Task> execute = () =>
            {
                ++executed;
                return tcs.Task;
            };
            var component = new DelegateCommandExecutor<object>(execute, null, CommandExecutionBehavior.None, value);
            var task1 = component.ExecuteAsync(_command, null, default, null);
            var task2 = component.ExecuteAsync(_command, null, default, null);
            executed.ShouldEqual(value ? 2 : 1);

            tcs.SetResult(this);
            await task1;
            await task2;
            await component.ExecuteAsync(_command, null, default, null);
            executed.ShouldEqual(value ? 3 : 2);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ShouldNotifyCanExecuteChangedAllowMultipleExecution(bool value)
        {
            var executed = 0;
            var tcs = new TaskCompletionSource<object>();
            Func<Task> execute = () => tcs.Task;

            var listener = new TestCommandEventHandlerComponent {RaiseCanExecuteChanged = c => { ++executed; }};
            _command.AddComponent(listener);

            var component = new DelegateCommandExecutor<object>(execute, null, CommandExecutionBehavior.None, value);
            _command.AddComponent(component);
            executed.ShouldEqual(1);
            executed = 0;
            var task = _command.ExecuteAsync(this);
            executed.ShouldEqual(value ? 0 : 1);

            tcs.SetResult(this);
            await task;
            executed.ShouldEqual(value ? 0 : 2);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task DisposeShouldClearDelegates(bool canDispose)
        {
            var executed = 0;
            Action execute = () => ++executed;
            var component = new DelegateCommandExecutor<object>(execute, null, CommandExecutionBehavior.None, true);
            component.IsDisposable.ShouldBeTrue();
            component.IsDisposable = canDispose;
            component.Dispose();

            component.CanExecute(_command, null, null).ShouldEqual(!canDispose);
            await component.ExecuteAsync(_command, null, default, null);
            executed.ShouldEqual(canDispose ? 0 : 1);
        }
    }
}