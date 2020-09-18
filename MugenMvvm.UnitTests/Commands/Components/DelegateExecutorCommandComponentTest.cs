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
    public class DelegateExecutorCommandComponentTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void ShouldSupportAction()
        {
            var cmd = new CompositeCommand();
            var executed = 0;
            Action execute = () => ++executed;
            var component = new DelegateExecutorCommandComponent<object>(execute, null, CommandExecutionMode.None, true);
            component.ExecuteAsync(cmd, null, null);
            executed.ShouldEqual(1);
        }

        [Fact]
        public void ShouldSupportActionWithObject()
        {
            var cmd = new CompositeCommand();
            var executed = 0;
            Action<object> execute = item =>
            {
                item.ShouldEqual(this);
                ++executed;
            };
            var component = new DelegateExecutorCommandComponent<object>(execute, null, CommandExecutionMode.None, true);
            component.ExecuteAsync(cmd, this, null);
            executed.ShouldEqual(1);
        }

        [Fact]
        public void ShouldSupportFuncTask()
        {
            var cmd = new CompositeCommand();
            var executed = 0;
            var tcs = new TaskCompletionSource<object>();
            Func<Task> execute = () =>
            {
                ++executed;
                return tcs.Task;
            };
            var component = new DelegateExecutorCommandComponent<object>(execute, null, CommandExecutionMode.None, true);
            var task = component.ExecuteAsync(cmd, null, null)!;
            executed.ShouldEqual(1);
            task.IsCompleted.ShouldBeFalse();
            tcs.SetResult(this);
            task.IsCompleted.ShouldBeTrue();
        }

        [Fact]
        public void ShouldSupportFuncTaskWithObject()
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
            var component = new DelegateExecutorCommandComponent<object>(execute, null, CommandExecutionMode.None, true);
            var task = component.ExecuteAsync(cmd, this, null)!;
            executed.ShouldEqual(1);
            task.IsCompleted.ShouldBeFalse();
            tcs.SetResult(this);
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
            var component = new DelegateExecutorCommandComponent<object>(execute, hasCanExecute ? canExecute : null, CommandExecutionMode.None, allowMultiply);
            component.HasCanExecute(cmd, null).ShouldEqual(value);
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
            var component = new DelegateExecutorCommandComponent<object>(execute, canExecute, CommandExecutionMode.None, true);
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
            var component = new DelegateExecutorCommandComponent<object>(execute, canExecute, CommandExecutionMode.None, true);
            component.CanExecute(cmd, this, null).ShouldEqual(canExecuteValue);
            executed.ShouldEqual(1);

            canExecuteValue = true;
            component.CanExecute(cmd, this, null).ShouldEqual(canExecuteValue);
            executed.ShouldEqual(2);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ShouldSupportAllowMultipleExecution(bool value)
        {
            var cmd = new CompositeCommand();
            var executed = 0;
            var tcs = new TaskCompletionSource<object>();
            Func<Task> execute = () =>
            {
                ++executed;
                return tcs.Task;
            };
            var component = new DelegateExecutorCommandComponent<object>(execute, null, CommandExecutionMode.None, value);
            component.ExecuteAsync(cmd, null, null);
            component.ExecuteAsync(cmd, null, null);
            executed.ShouldEqual(value ? 2 : 1);

            tcs.SetResult(this);
            component.ExecuteAsync(cmd, null, null);
            executed.ShouldEqual(value ? 3 : 2);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ShouldNotifyCanExecuteChangedAllowMultipleExecution(bool value)
        {
            var executed = 0;
            var tcs = new TaskCompletionSource<object>();
            Func<Task> execute = () => tcs.Task;
            var cmd = new CompositeCommand();

            var listener = new TestConditionEventCommandComponent {RaiseCanExecuteChanged = c => { ++executed; }};
            cmd.AddComponent(listener);

            var component = new DelegateExecutorCommandComponent<object>(execute, null, CommandExecutionMode.None, value);
            cmd.AddComponent(component);

            executed.ShouldEqual(1);
            executed = 0;
            cmd.Execute(this);
            executed.ShouldEqual(value ? 0 : 1);

            tcs.SetResult(this);
            executed.ShouldEqual(value ? 0 : 2);
        }

        [Fact]
        public void ShouldSupportCommandExecutionModeNone()
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
            var component = new DelegateExecutorCommandComponent<object>(execute, canExecute, CommandExecutionMode.None, true);
            cmd.AddComponent(component);

            component.ExecuteAsync(cmd, null, null);
            component.ExecuteAsync(cmd, null, null);
            executed.ShouldEqual(2);
            canExecuted.ShouldEqual(0);
        }

        [Fact]
        public void ShouldSupportCommandExecutionModeCanExecuteBeforeExecute()
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
            var component = new DelegateExecutorCommandComponent<object>(execute, canExecute, CommandExecutionMode.CanExecuteBeforeExecute, true);
            cmd.AddComponent(component);

            component.ExecuteAsync(cmd, null, null);
            executed.ShouldEqual(0);
            canExecuted.ShouldEqual(1);

            canExecuteValue = true;
            component.ExecuteAsync(cmd, null, null);
            executed.ShouldEqual(1);
            canExecuted.ShouldEqual(2);
        }

        [Fact]
        public void ShouldSupportCommandExecutionModeCanExecuteBeforeExecuteException()
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
            var component = new DelegateExecutorCommandComponent<object>(execute, canExecute, CommandExecutionMode.CanExecuteBeforeExecuteException, true);
            cmd.AddComponent(component);

            var task = component.ExecuteAsync(cmd, null, null)!;
            task.IsFaulted.ShouldBeTrue();
            task.Exception!.GetBaseException().ShouldBeType<InvalidOperationException>();
            executed.ShouldEqual(0);
            canExecuted.ShouldEqual(1);

            canExecuteValue = true;
            task = component.ExecuteAsync(cmd, null, null) ?? Default.CompletedTask;
            task.Wait();
            executed.ShouldEqual(1);
            canExecuted.ShouldEqual(2);
        }

        [Fact]
        public void ShouldNotThrowException()
        {
            var cmd = new CompositeCommand();
            var exception = new NotSupportedException();
            Action execute = () => throw exception;
            var component = new DelegateExecutorCommandComponent<object>(execute, null, CommandExecutionMode.None, true);
            var task = component.ExecuteAsync(cmd, null, null)!;
            task.IsFaulted.ShouldBeTrue();
            task.Exception!.GetBaseException().ShouldEqual(exception);
        }

        [Fact]
        public void DisposeShouldClearDelegates()
        {
            var cmd = new CompositeCommand();
            var executed = 0;
            Action execute = () => ++executed;
            var component = new DelegateExecutorCommandComponent<object>(execute, null, CommandExecutionMode.None, true);
            component.Dispose();

            component.CanExecute(cmd, null, null).ShouldBeFalse();
            component.ExecuteAsync(cmd, null, null);
            executed.ShouldEqual(0);
        }

        #endregion
    }
}