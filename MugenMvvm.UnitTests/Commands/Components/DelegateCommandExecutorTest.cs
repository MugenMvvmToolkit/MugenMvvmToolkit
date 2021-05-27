#pragma warning disable 4014
using System;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Commands;
using MugenMvvm.Commands.Components;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Metadata;
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
            Action<IReadOnlyMetadataContext?> execute = m => throw exception;
            var component = new DelegateCommandExecutor<object>(execute, null, CommandExecutionBehavior.None, true);
            var task = component.ExecuteAsync(_command, null, default, null).AsTask();
            task.IsFaulted.ShouldBeTrue();
            task.Exception!.GetBaseException().ShouldEqual(exception);
        }

        [Fact]
        public async Task ShouldSupportAction()
        {
            var executed = 0;
            Action<IReadOnlyMetadataContext?> execute = m =>
            {
                m.ShouldEqual(DefaultMetadata);
                ++executed;
            };
            var component = new DelegateCommandExecutor<object>(execute, null, CommandExecutionBehavior.None, true);
            await component.ExecuteAsync(_command, null, default, DefaultMetadata);
            executed.ShouldEqual(1);
        }

        [Fact]
        public async Task ShouldSupportActionWithObject()
        {
            var executed = 0;
            Action<object, IReadOnlyMetadataContext?> execute = (item, m) =>
            {
                item.ShouldEqual(this);
                m.ShouldEqual(DefaultMetadata);
                ++executed;
            };
            var component = new DelegateCommandExecutor<object>(execute, null, CommandExecutionBehavior.None, true);
            await component.ExecuteAsync(_command, this, default, DefaultMetadata);
            executed.ShouldEqual(1);
        }

        [Fact]
        public void ShouldSupportCanExecuteFunc()
        {
            var executed = 0;
            var canExecuteValue = false;
            Action<IReadOnlyMetadataContext?> execute = m => { };
            Func<IReadOnlyMetadataContext?, bool> canExecute = m =>
            {
                ++executed;
                m.ShouldEqual(DefaultMetadata);
                return canExecuteValue;
            };
            var component = new DelegateCommandExecutor<object>(execute, canExecute, CommandExecutionBehavior.None, true);
            component.CanExecute(_command, null, DefaultMetadata).ShouldEqual(canExecuteValue);
            executed.ShouldEqual(1);

            canExecuteValue = true;
            component.CanExecute(_command, null, DefaultMetadata).ShouldEqual(canExecuteValue);
            executed.ShouldEqual(2);
        }

        [Fact]
        public void ShouldSupportCanExecuteFuncWithObject()
        {
            var executed = 0;
            var canExecuteValue = false;
            Action execute = () => { };
            Func<object?, IReadOnlyMetadataContext?, bool> canExecute = (item, m) =>
            {
                item.ShouldEqual(this);
                m.ShouldEqual(DefaultMetadata);
                ++executed;
                return canExecuteValue;
            };
            var component = new DelegateCommandExecutor<object>(execute, canExecute, CommandExecutionBehavior.None, true);
            component.CanExecute(_command, this, DefaultMetadata).ShouldEqual(canExecuteValue);
            executed.ShouldEqual(1);

            canExecuteValue = true;
            component.CanExecute(_command, this, DefaultMetadata).ShouldEqual(canExecuteValue);
            executed.ShouldEqual(2);
        }

        [Fact]
        public async Task ShouldSupportCommandExecutionModeCanExecuteBeforeExecute()
        {
            var executed = 0;
            var canExecuted = 0;
            Action<IReadOnlyMetadataContext?> execute = (m) =>
            {
                m.ShouldEqual(DefaultMetadata);
                ++executed;
            };
            var canExecuteValue = false;
            Func<IReadOnlyMetadataContext?, bool> canExecute = m =>
            {
                ++canExecuted;
                m.ShouldEqual(DefaultMetadata);
                return canExecuteValue;
            };

            var component = new DelegateCommandExecutor<object>(execute, canExecute, CommandExecutionBehavior.CheckCanExecute, true);
            _command.AddComponent(component);

            await component.ExecuteAsync(_command, null, default, DefaultMetadata);
            executed.ShouldEqual(0);
            canExecuted.ShouldEqual(1);

            canExecuteValue = true;
            await component.ExecuteAsync(_command, null, default, DefaultMetadata);
            executed.ShouldEqual(1);
            canExecuted.ShouldEqual(2);
        }

        [Fact]
        public async Task ShouldSupportCommandExecutionModeCanExecuteBeforeExecuteException()
        {
            var executed = 0;
            var canExecuted = 0;
            Action<IReadOnlyMetadataContext?> execute = m =>
            {
                m.ShouldEqual(DefaultMetadata);
                ++executed;
            };
            var canExecuteValue = false;
            Func<IReadOnlyMetadataContext?, bool> canExecute = (m) =>
            {
                m.ShouldEqual(DefaultMetadata);
                ++canExecuted;
                return canExecuteValue;
            };

            var component = new DelegateCommandExecutor<object>(execute, canExecute, CommandExecutionBehavior.CheckCanExecuteThrow, true);
            _command.AddComponent(component);

            var task = component.ExecuteAsync(_command, null, default, DefaultMetadata).AsTask();
            task.IsFaulted.ShouldBeTrue();
            task.Exception!.GetBaseException().ShouldBeType<InvalidOperationException>();
            executed.ShouldEqual(0);
            canExecuted.ShouldEqual(1);

            canExecuteValue = true;
            task = component.ExecuteAsync(_command, null, default, DefaultMetadata).AsTask();
            (await task).ShouldBeTrue();
            executed.ShouldEqual(1);
            canExecuted.ShouldEqual(2);
        }

        [Fact]
        public async Task ShouldSupportCommandExecutionModeNone()
        {
            var executed = 0;
            var canExecuted = 0;
            Action<IReadOnlyMetadataContext?> execute = (m) =>
            {
                m.ShouldEqual(DefaultMetadata);
                ++executed;
            };
            var canExecuteValue = false;
            Func<IReadOnlyMetadataContext?, bool> canExecute = m =>
            {
                ++canExecuted;
                m.ShouldEqual(DefaultMetadata);
                return canExecuteValue;
            };

            var component = new DelegateCommandExecutor<object>(execute, canExecute, CommandExecutionBehavior.None, true);
            _command.AddComponent(component);

            await component.ExecuteAsync(_command, null, default, DefaultMetadata);
            await component.ExecuteAsync(_command, null, default, DefaultMetadata);
            executed.ShouldEqual(2);
            canExecuted.ShouldEqual(0);
        }

        [Fact]
        public async Task ShouldSupportFuncTask()
        {
            var executed = 0;
            var tcs = new TaskCompletionSource<object>();
            Func<CancellationToken, IReadOnlyMetadataContext?, Task> execute = (c, m) =>
            {
                ++executed;
                c.ShouldEqual(DefaultCancellationToken);
                m.ShouldEqual(DefaultMetadata);
                return tcs.Task;
            };
            var component = new DelegateCommandExecutor<object>(execute, null, CommandExecutionBehavior.None, true);
            var task = component.ExecuteAsync(_command, null, DefaultCancellationToken, DefaultMetadata);
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
            Func<object?, CancellationToken, IReadOnlyMetadataContext?, Task> execute = (item, c, m) =>
            {
                item.ShouldEqual(this);
                c.ShouldEqual(DefaultCancellationToken);
                m.ShouldEqual(DefaultMetadata);
                ++executed;
                return tcs.Task;
            };
            var component = new DelegateCommandExecutor<object>(execute, null, CommandExecutionBehavior.None, true);
            var task = component.ExecuteAsync(_command, this, DefaultCancellationToken, DefaultMetadata);
            executed.ShouldEqual(1);
            task.IsCompleted.ShouldBeFalse();
            tcs.SetResult(this);
            await task;
            task.IsCompleted.ShouldBeTrue();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ShouldSupportAllowMultipleExecution(bool value)
        {
            var executed = 0;
            var tcs = new TaskCompletionSource<object>();
            Func<CancellationToken, IReadOnlyMetadataContext?, Task> execute = (c, m) =>
            {
                ++executed;
                c.ShouldEqual(DefaultCancellationToken);
                m.ShouldEqual(DefaultMetadata);
                return tcs.Task;
            };
            var component = new DelegateCommandExecutor<object>(execute, null, CommandExecutionBehavior.None, value);
            var task1 = component.ExecuteAsync(_command, null, DefaultCancellationToken, DefaultMetadata);
            var task2 = component.ExecuteAsync(_command, null, DefaultCancellationToken, DefaultMetadata);

            component.CanExecute(_command, null, DefaultMetadata).ShouldEqual(value);
            tcs.SetResult(this);
            await task1;
            await task2;
            executed.ShouldEqual(value ? 2 : 1);
            await component.ExecuteAsync(_command, null, DefaultCancellationToken, DefaultMetadata);
            executed.ShouldEqual(value ? 3 : 2);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ShouldSupportAllowMultipleExecutionForceExecute(bool value)
        {
            var context = CommandMetadata.ForceExecute.ToContext(true);
            var executed = 0;
            var tcs = new TaskCompletionSource<object>();
            Func<CancellationToken, IReadOnlyMetadataContext?, Task> execute = (c, m) =>
            {
                ++executed;
                c.ShouldEqual(DefaultCancellationToken);
                m.ShouldEqual(context);
                return tcs.Task;
            };
            var component = new DelegateCommandExecutor<object>(execute, null, CommandExecutionBehavior.None, value);
            var task1 = component.ExecuteAsync(_command, null, DefaultCancellationToken, context);
            var task2 = component.ExecuteAsync(_command, null, DefaultCancellationToken, context);

            component.CanExecute(_command, null, context).ShouldBeTrue();
            context = DefaultMetadata;
            component.CanExecute(_command, null, DefaultMetadata).ShouldEqual(value);
            tcs.SetResult(this);
            await task1;
            await task2;
            executed.ShouldEqual(2);
            await component.ExecuteAsync(_command, null, DefaultCancellationToken, DefaultMetadata);
            executed.ShouldEqual(3);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ShouldNotifyCanExecuteChangedAllowMultipleExecution(bool value)
        {
            var executed = 0;
            var tcs = new TaskCompletionSource<object>();
            Func<CancellationToken, IReadOnlyMetadataContext?, Task> execute = (c, m) => tcs.Task;

            var listener = new TestCommandEventHandlerComponent {RaiseCanExecuteChanged = c => { ++executed; }};
            _command.AddComponent(listener);

            var component = new DelegateCommandExecutor<object>(execute, null, CommandExecutionBehavior.None, value);
            _command.AddComponent(component);
            executed.ShouldEqual(1);
            executed = 0;
            var task = _command.ExecuteAsync(this, DefaultCancellationToken, DefaultMetadata);
            executed.ShouldEqual(value ? 0 : 1);

            component.CanExecute(_command, null, DefaultMetadata).ShouldEqual(value);
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
            Action<IReadOnlyMetadataContext?> execute = m => ++executed;
            var component = new DelegateCommandExecutor<object>(execute, null, CommandExecutionBehavior.None, true);
            component.IsDisposable.ShouldBeTrue();
            component.IsDisposable = canDispose;
            component.Dispose();

            component.CanExecute(_command, null, DefaultMetadata).ShouldEqual(!canDispose);
            await component.ExecuteAsync(_command, null, DefaultCancellationToken, DefaultMetadata);
            executed.ShouldEqual(canDispose ? 0 : 1);
        }
    }
}