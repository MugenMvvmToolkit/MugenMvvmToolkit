#pragma warning disable 4014
using System;
using System.Threading.Tasks;
using MugenMvvm.Commands;
using MugenMvvm.Commands.Components;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Internal;
using MugenMvvm.UnitTests.Commands.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Commands.Components
{
    public class DelegateCommandExecutorTest : UnitTestBase
    {
        [Fact]
        public void ShouldNotThrowException()
        {
            var cmd = new CompositeCommand();
            var exception = new NotSupportedException();
            Action execute = () => throw exception;
            var component = new DelegateCommandExecutor<object>(execute, null, CommandExecutionBehavior.None, true);
            var task = component.ExecuteAsync(cmd, null, default, null)!;
            task.IsFaulted.ShouldBeTrue();
            task.Exception!.GetBaseException().ShouldEqual(exception);
        }

        [Fact]
        public async Task ShouldSupportAction()
        {
            var cmd = new CompositeCommand();
            var executed = 0;
            Action execute = () => ++executed;
            var component = new DelegateCommandExecutor<object>(execute, null, CommandExecutionBehavior.None, true);
            await component.ExecuteAsync(cmd, null, default, null);
            executed.ShouldEqual(1);
        }

        [Fact]
        public async Task ShouldSupportActionWithObject()
        {
            var cmd = new CompositeCommand();
            var executed = 0;
            Action<object> execute = item =>
            {
                item.ShouldEqual(this);
                ++executed;
            };
            var component = new DelegateCommandExecutor<object>(execute, null, CommandExecutionBehavior.None, true);
            await component.ExecuteAsync(cmd, this, default, null);
            executed.ShouldEqual(1);
        }

        [Fact]
        public void ShouldSupportCanExecuteFunc()
        {
            var cmd = new CompositeCommand();
            var executed = 0;
            var canExecuteValue = false;
            Action execute = () => { };
            Func<bool> canExecute = () =>
            {
                ++executed;
                return canExecuteValue;
            };
            var component = new DelegateCommandExecutor<object>(execute, canExecute, CommandExecutionBehavior.None, true);
            component.CanExecute(cmd, null, null).ShouldEqual(canExecuteValue);
            executed.ShouldEqual(1);

            canExecuteValue = true;
            component.CanExecute(cmd, null, null).ShouldEqual(canExecuteValue);
            executed.ShouldEqual(2);
        }

        [Fact]
        public void ShouldSupportCanExecuteFuncWithObject()
        {
            var cmd = new CompositeCommand();
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
            component.CanExecute(cmd, this, null).ShouldEqual(canExecuteValue);
            executed.ShouldEqual(1);

            canExecuteValue = true;
            component.CanExecute(cmd, this, null).ShouldEqual(canExecuteValue);
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

            var cmd = new CompositeCommand();
            var component = new DelegateCommandExecutor<object>(execute, canExecute, CommandExecutionBehavior.CheckCanExecute, true);
            cmd.AddComponent(component);

            await component.ExecuteAsync(cmd, null, default, null);
            executed.ShouldEqual(0);
            canExecuted.ShouldEqual(1);

            canExecuteValue = true;
            await component.ExecuteAsync(cmd, null, default, null);
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

            var cmd = new CompositeCommand();
            var component = new DelegateCommandExecutor<object>(execute, canExecute, CommandExecutionBehavior.CheckCanExecuteThrow, true);
            cmd.AddComponent(component);

            var task = component.ExecuteAsync(cmd, null, default, null)!;
            task.IsFaulted.ShouldBeTrue();
            task.Exception!.GetBaseException().ShouldBeType<InvalidOperationException>();
            executed.ShouldEqual(0);
            canExecuted.ShouldEqual(1);

            canExecuteValue = true;
            task = component.ExecuteAsync(cmd, null, default, null) ?? Default.CompletedTask;
            await task;
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

            var cmd = new CompositeCommand();
            var component = new DelegateCommandExecutor<object>(execute, canExecute, CommandExecutionBehavior.None, true);
            cmd.AddComponent(component);

            await component.ExecuteAsync(cmd, null, default, null);
            await component.ExecuteAsync(cmd, null, default, null);
            executed.ShouldEqual(2);
            canExecuted.ShouldEqual(0);
        }

        [Fact]
        public async Task ShouldSupportFuncTask()
        {
            var cmd = new CompositeCommand();
            var executed = 0;
            var tcs = new TaskCompletionSource<object>();
            Func<Task> execute = () =>
            {
                ++executed;
                return tcs.Task;
            };
            var component = new DelegateCommandExecutor<object>(execute, null, CommandExecutionBehavior.None, true);
            var task = component.ExecuteAsync(cmd, null, default, null)!;
            executed.ShouldEqual(1);
            task.IsCompleted.ShouldBeFalse();
            tcs.SetResult(this);
            await task;
            task.IsCompleted.ShouldBeTrue();
        }

        [Fact]
        public async Task ShouldSupportFuncTaskWithObject()
        {
            var cmd = new CompositeCommand();
            var executed = 0;
            var tcs = new TaskCompletionSource<object>();
            Func<object?, Task> execute = item =>
            {
                item.ShouldEqual(this);
                ++executed;
                return tcs.Task;
            };
            var component = new DelegateCommandExecutor<object>(execute, null, CommandExecutionBehavior.None, true);
            var task = component.ExecuteAsync(cmd, this, default, null)!;
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
            var cmd = new CompositeCommand();
            Action execute = () => { };
            Func<bool> canExecute = () => true;
            var component = new DelegateCommandExecutor<object>(execute, hasCanExecute ? canExecute : null, CommandExecutionBehavior.None, allowMultiply);
            component.HasCanExecute(cmd, null).ShouldEqual(value);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ShouldSupportAllowMultipleExecution(bool value)
        {
            var cmd = new CompositeCommand();
            var executed = 0;
            var tcs = new TaskCompletionSource<object>();
            Func<Task> execute = () =>
            {
                ++executed;
                return tcs.Task;
            };
            var component = new DelegateCommandExecutor<object>(execute, null, CommandExecutionBehavior.None, value);
            var task1 = component.ExecuteAsync(cmd, null, default, null);
            var task2 = component.ExecuteAsync(cmd, null, default, null);
            executed.ShouldEqual(value ? 2 : 1);

            tcs.SetResult(this);
            await task1;
            await task2;
            await component.ExecuteAsync(cmd, null, default, null);
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
            var cmd = new CompositeCommand();

            var listener = new TestCommandEventHandlerComponent {RaiseCanExecuteChanged = c => { ++executed; }};
            cmd.AddComponent(listener);

            var component = new DelegateCommandExecutor<object>(execute, null, CommandExecutionBehavior.None, value);
            cmd.AddComponent(component);
            executed.ShouldEqual(1);
            executed = 0;
            var task = cmd.ExecuteAsync(this);
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
            var cmd = new CompositeCommand();
            var executed = 0;
            Action execute = () => ++executed;
            var component = new DelegateCommandExecutor<object>(execute, null, CommandExecutionBehavior.None, true);
            component.IsDisposable.ShouldBeTrue();
            component.IsDisposable = canDispose;
            component.Dispose();

            component.CanExecute(cmd, null, null).ShouldEqual(!canDispose);
            await component.ExecuteAsync(cmd, null, default, null);
            executed.ShouldEqual(canDispose ? 0 : 1);
        }
    }
}