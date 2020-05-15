using System;
using System.Threading.Tasks;
using MugenMvvm.Commands;
using MugenMvvm.Commands.Components;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.UnitTest.Commands.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Commands.Components
{
    public class DelegateExecutorCommandComponentTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void ShouldSupportAction()
        {
            var executed = 0;
            Action execute = () => ++executed;
            var component = new DelegateExecutorCommandComponent<object>(execute, null, CommandExecutionMode.None, true);
            component.ExecuteAsync(null);
            executed.ShouldEqual(1);
        }

        [Fact]
        public void ShouldSupportActionWithObject()
        {
            var executed = 0;
            Action<object> execute = item =>
            {
                item.ShouldEqual(this);
                ++executed;
            };
            var component = new DelegateExecutorCommandComponent<object>(execute, null, CommandExecutionMode.None, true);
            component.ExecuteAsync(this);
            executed.ShouldEqual(1);
        }

        [Fact]
        public void ShouldSupportFuncTask()
        {
            var executed = 0;
            var tcs = new TaskCompletionSource<object>();
            Func<Task> execute = () =>
            {
                ++executed;
                return tcs.Task;
            };
            var component = new DelegateExecutorCommandComponent<object>(execute, null, CommandExecutionMode.None, true);
            var task = component.ExecuteAsync(null);
            executed.ShouldEqual(1);
            task.IsCompleted.ShouldBeFalse();
            tcs.SetResult(this);
            task.IsCompleted.ShouldBeTrue();
        }

        [Fact]
        public void ShouldSupportFuncTaskWithObject()
        {
            var executed = 0;
            var tcs = new TaskCompletionSource<object>();
            Func<object?, Task> execute = item =>
            {
                item.ShouldEqual(this);
                ++executed;
                return tcs.Task;
            };
            var component = new DelegateExecutorCommandComponent<object>(execute, null, CommandExecutionMode.None, true);
            var task = component.ExecuteAsync(this);
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
            Action execute = () => { };
            Func<bool> canExecute = () => true;
            var component = new DelegateExecutorCommandComponent<object>(execute, hasCanExecute ? canExecute : null, CommandExecutionMode.None, allowMultiply);
            component.HasCanExecute().ShouldEqual(value);
        }

        [Fact]
        public void ShouldSupportCanExecuteFunc()
        {
            var executed = 0;
            bool canExecuteValue = false;
            Action execute = () => { };
            Func<bool> canExecute = () =>
            {
                ++executed;
                return canExecuteValue;
            };
            var component = new DelegateExecutorCommandComponent<object>(execute, canExecute, CommandExecutionMode.None, true);
            component.CanExecute(null).ShouldEqual(canExecuteValue);
            executed.ShouldEqual(1);

            canExecuteValue = true;
            component.CanExecute(null).ShouldEqual(canExecuteValue);
            executed.ShouldEqual(2);
        }

        [Fact]
        public void ShouldSupportCanExecuteFuncWithObject()
        {
            var executed = 0;
            bool canExecuteValue = false;
            Action execute = () => { };
            Func<object?, bool> canExecute = item =>
            {
                item.ShouldEqual(this);
                ++executed;
                return canExecuteValue;
            };
            var component = new DelegateExecutorCommandComponent<object>(execute, canExecute, CommandExecutionMode.None, true);
            component.CanExecute(this).ShouldEqual(canExecuteValue);
            executed.ShouldEqual(1);

            canExecuteValue = true;
            component.CanExecute(this).ShouldEqual(canExecuteValue);
            executed.ShouldEqual(2);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ShouldSupportAllowMultipleExecution(bool value)
        {
            var executed = 0;
            var tcs = new TaskCompletionSource<object>();
            Func<Task> execute = () =>
            {
                ++executed;
                return tcs.Task;
            };
            var component = new DelegateExecutorCommandComponent<object>(execute, null, CommandExecutionMode.None, value);
            component.ExecuteAsync(null);
            component.ExecuteAsync(null);
            executed.ShouldEqual(value ? 2 : 1);

            tcs.SetResult(this);
            component.ExecuteAsync(null);
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
            var compositeCommand = new CompositeCommand();

            var listener = new TestConditionEventCommandComponent { RaiseCanExecuteChanged = () => { ++executed; } };
            compositeCommand.AddComponent(listener);

            var component = new DelegateExecutorCommandComponent<object>(execute, null, CommandExecutionMode.None, value);
            compositeCommand.AddComponent(component);

            executed.ShouldEqual(1);
            executed = 0;
            compositeCommand.Execute(this);
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
            bool canExecuteValue = false;
            Func<bool> canExecute = () =>
            {
                ++canExecuted;
                return canExecuteValue;
            };

            var compositeCommand = new CompositeCommand();
            var component = new DelegateExecutorCommandComponent<object>(execute, canExecute, CommandExecutionMode.None, true);
            compositeCommand.AddComponent(component);

            component.ExecuteAsync(null);
            component.ExecuteAsync(null);
            executed.ShouldEqual(2);
            canExecuted.ShouldEqual(0);
        }

        [Fact]
        public void ShouldSupportCommandExecutionModeCanExecuteBeforeExecute()
        {
            var executed = 0;
            var canExecuted = 0;
            Action execute = () => ++executed;
            bool canExecuteValue = false;
            Func<bool> canExecute = () =>
            {
                ++canExecuted;
                return canExecuteValue;
            };

            var compositeCommand = new CompositeCommand();
            var component = new DelegateExecutorCommandComponent<object>(execute, canExecute, CommandExecutionMode.CanExecuteBeforeExecute, true);
            compositeCommand.AddComponent(component);

            component.ExecuteAsync(null);
            executed.ShouldEqual(0);
            canExecuted.ShouldEqual(1);

            canExecuteValue = true;
            component.ExecuteAsync(null);
            executed.ShouldEqual(1);
            canExecuted.ShouldEqual(2);
        }

        [Fact]
        public void ShouldSupportCommandExecutionModeCanExecuteBeforeExecuteException()
        {
            var executed = 0;
            var canExecuted = 0;
            Action execute = () => ++executed;
            bool canExecuteValue = false;
            Func<bool> canExecute = () =>
            {
                ++canExecuted;
                return canExecuteValue;
            };

            var compositeCommand = new CompositeCommand();
            var component = new DelegateExecutorCommandComponent<object>(execute, canExecute, CommandExecutionMode.CanExecuteBeforeExecuteException, true);
            compositeCommand.AddComponent(component);

            ShouldThrow<InvalidOperationException>(() => component.ExecuteAsync(null));
            executed.ShouldEqual(0);
            canExecuted.ShouldEqual(1);

            canExecuteValue = true;
            component.ExecuteAsync(null);
            executed.ShouldEqual(1);
            canExecuted.ShouldEqual(2);
        }

        [Fact]
        public void ShouldReThrowException()
        {
            Action execute = () => throw new NotSupportedException();
            var component = new DelegateExecutorCommandComponent<object>(execute, null, CommandExecutionMode.None, true);
            ShouldThrow<NotSupportedException>(() => component.ExecuteAsync(null));
        }

        [Fact]
        public void DisposeShouldClearDelegates()
        {
            int executed = 0;
            Action execute = () => ++executed;
            Func<bool> canExecute = () => true;
            var component = new DelegateExecutorCommandComponent<object>(execute, null, CommandExecutionMode.None, true);
            component.Dispose();

            component.CanExecute(null).ShouldBeFalse();
            component.ExecuteAsync(null);
            executed.ShouldEqual(0);
        }

        #endregion
    }
}