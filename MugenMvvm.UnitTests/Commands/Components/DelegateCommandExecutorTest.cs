﻿#pragma warning disable 4014
using System;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Commands.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Commands;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models.Components;
using MugenMvvm.Metadata;
using MugenMvvm.Tests.Commands;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Commands.Components
{
    public class DelegateCommandExecutorTest : UnitTestBase
    {
        public DelegateCommandExecutorTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task DisposeShouldClearDelegates(bool allowMultipleExecution)
        {
            var executed = 0;
            Action<IReadOnlyMetadataContext?> execute = m => ++executed;
            var component = Add<object>(Command, execute, null, allowMultipleExecution);
            Command.GetComponents<IDisposableComponent<ICompositeCommand>>().OnDisposed(Command, null);

            component.CanExecute(Command, null, Metadata).ShouldBeFalse();
            await component.TryExecuteAsync(Command, null, DefaultCancellationToken, Metadata);
            executed.ShouldEqual(0);
        }

        [Fact]
        public async Task ShouldCancelPreviousTokenForceExecute()
        {
            var executed = 0;
            Func<CancellationToken, IReadOnlyMetadataContext?, Task> execute = (c, _) =>
            {
                ++executed;
                var source = new TaskCompletionSource<object>();
                c.Register(() => source.TrySetCanceled(c));
                return source.Task;
            };
            var component = Add<object>(Command, execute, null, false);
            var task1 = component.TryExecuteAsync(Command, null, DefaultCancellationToken, null);
            executed.ShouldEqual(1);

            var task2 = component.TryExecuteAsync(Command, null, DefaultCancellationToken, MugenExtensions.GetForceExecuteMetadata(null));
            await task1.WaitSafeAsync();
            task1.IsCanceled.ShouldBeTrue();
            task2.IsCompleted.ShouldBeFalse();
            executed.ShouldEqual(2);
        }

        [Fact]
        public async Task ShouldWaitLastExecute()
        {
            var executed = 0;
            var taskCompletionSource = new TaskCompletionSource<object>();
            Func<CancellationToken, IReadOnlyMetadataContext?, Task> execute = (c, _) =>
            {
                ++executed;
                return taskCompletionSource.Task;
            };
            var component = Add<object>(Command, execute, null, false);
            var task1 = component.TryExecuteAsync(Command, null, DefaultCancellationToken, null);
            executed.ShouldEqual(1);

            var task2 = component.TryWaitAsync(Command, null);
            executed.ShouldEqual(1);

            task1.IsCompleted.ShouldBeFalse();
            task2.IsCompleted.ShouldBeFalse();

            taskCompletionSource.TrySetResult(null!);
            await task1;
            await task2;
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ShouldNotifyCanExecuteChangedAllowMultipleExecution(bool allowMultipleExecution)
        {
            var executed = 0;
            var tcs = new TaskCompletionSource<object>();
            Func<CancellationToken, IReadOnlyMetadataContext?, Task> execute = (c, m) => tcs.Task;

            var listener = new TestCommandEventHandlerComponent {RaiseCanExecuteChanged = c => ++executed};
            Command.AddComponent(listener);

            var component = Add<object>(Command, execute, null, allowMultipleExecution);
            executed.ShouldEqual(1);
            executed = 0;
            var task = Command.ExecuteAsync(this, DefaultCancellationToken, Metadata);
            executed.ShouldEqual(1);

            component.CanExecute(Command, null, Metadata).ShouldEqual(allowMultipleExecution);
            tcs.SetResult(this);
            await task;
            executed.ShouldEqual(2);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ShouldNotThrowException(bool allowMultipleExecution)
        {
            var exception = new NotSupportedException();
            Action<IReadOnlyMetadataContext?> execute = m => throw exception;
            var component = Add<object>(Command, execute, null, allowMultipleExecution);
            var task = component.TryExecuteAsync(Command, null, default, null);
            task.IsFaulted.ShouldBeTrue();
            task.Exception!.GetBaseException().ShouldEqual(exception);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ShouldSupportAction(bool allowMultipleExecution)
        {
            var executed = 0;
            Action<IReadOnlyMetadataContext?> execute = m =>
            {
                m.ShouldEqual(Metadata);
                ++executed;
            };
            var component = Add<object>(Command, execute, null, allowMultipleExecution);
            await component.TryExecuteAsync(Command, null, default, Metadata);
            executed.ShouldEqual(1);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ShouldSupportActionWithObject(bool allowMultipleExecution)
        {
            var executed = 0;
            Action<object, IReadOnlyMetadataContext?> execute = (item, m) =>
            {
                item.ShouldEqual(this);
                m.ShouldEqual(Metadata);
                ++executed;
            };
            var component = Add<object>(Command, execute, null, allowMultipleExecution);
            await component.TryExecuteAsync(Command, this, default, Metadata);
            executed.ShouldEqual(1);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ShouldSupportAllowMultipleExecution(bool allowMultipleExecution)
        {
            var executed = 0;
            var tcs = new TaskCompletionSource<object>();
            Func<CancellationToken, IReadOnlyMetadataContext?, Task> execute = (c, m) =>
            {
                ++executed;
                if (allowMultipleExecution)
                    c.ShouldEqual(DefaultCancellationToken);
                else
                    c.CanBeCanceled.ShouldBeTrue();
                m.ShouldEqual(Metadata);
                return tcs.Task;
            };
            var component = Add<object>(Command, execute, null, allowMultipleExecution);
            var task1 = component.TryExecuteAsync(Command, null, DefaultCancellationToken, Metadata);
            var task2 = component.TryExecuteAsync(Command, null, DefaultCancellationToken, Metadata);

            component.CanExecute(Command, null, Metadata).ShouldEqual(allowMultipleExecution);
            tcs.SetResult(this);
            await task1;
            await task2;
            executed.ShouldEqual(allowMultipleExecution ? 2 : 1);
            await component.TryExecuteAsync(Command, null, DefaultCancellationToken, Metadata);
            executed.ShouldEqual(allowMultipleExecution ? 3 : 2);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ShouldSupportAllowMultipleExecutionForceExecute(bool allowMultipleExecution)
        {
            var context = CommandMetadata.ForceExecute.ToContext(true);
            var executed = 0;
            var tcs = new TaskCompletionSource<object>();
            Func<CancellationToken, IReadOnlyMetadataContext?, Task> execute = (c, m) =>
            {
                ++executed;
                if (allowMultipleExecution)
                    c.ShouldEqual(DefaultCancellationToken);
                else
                    c.CanBeCanceled.ShouldBeTrue();
                m.ShouldEqual(context);
                return tcs.Task;
            };
            var component = Add<object>(Command, execute, null, allowMultipleExecution);
            var task1 = component.TryExecuteAsync(Command, null, DefaultCancellationToken, context);
            var task2 = component.TryExecuteAsync(Command, null, DefaultCancellationToken, context);

            component.CanExecute(Command, null, context).ShouldBeTrue();
            context = Metadata;
            component.CanExecute(Command, null, Metadata).ShouldEqual(allowMultipleExecution);
            tcs.SetResult(this);
            await task1;
            await task2;
            executed.ShouldEqual(2);
            await component.TryExecuteAsync(Command, null, DefaultCancellationToken, Metadata);
            executed.ShouldEqual(3);
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(false, false)]
        public async Task ShouldSupportBoolTaskFuncTask(bool result, bool allowMultipleExecution)
        {
            var executed = 0;
            var tcs = new TaskCompletionSource<bool>();
            Func<CancellationToken, IReadOnlyMetadataContext?, Task<bool>> execute = (c, m) =>
            {
                ++executed;
                if (allowMultipleExecution)
                    c.ShouldEqual(DefaultCancellationToken);
                else
                    c.CanBeCanceled.ShouldBeTrue();
                m.ShouldEqual(Metadata);
                return tcs.Task;
            };
            var component = Add<object>(Command, execute, null, allowMultipleExecution);
            var task = component.TryExecuteAsync(Command, null, DefaultCancellationToken, Metadata);
            executed.ShouldEqual(1);
            task.IsCompleted.ShouldBeFalse();
            tcs.SetResult(result);
            (await task).ShouldEqual(result);
            task.IsCompleted.ShouldBeTrue();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ShouldSupportCanExecuteFunc(bool allowMultipleExecution)
        {
            var executed = 0;
            var canExecuteValue = false;
            Action<IReadOnlyMetadataContext?> execute = m => { };
            Func<IReadOnlyMetadataContext?, bool> canExecute = m =>
            {
                ++executed;
                m.ShouldEqual(Metadata);
                return canExecuteValue;
            };
            var component = Add<object>(Command, execute, canExecute, allowMultipleExecution);
            component.CanExecute(Command, null, Metadata).ShouldEqual(canExecuteValue);
            executed.ShouldEqual(1);

            canExecuteValue = true;
            component.CanExecute(Command, null, Metadata).ShouldEqual(canExecuteValue);
            executed.ShouldEqual(2);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ShouldSupportCanExecuteFuncWithObject(bool allowMultipleExecution)
        {
            var executed = 0;
            var canExecuteValue = false;
            Action execute = () => { };
            Func<object?, IReadOnlyMetadataContext?, bool> canExecute = (item, m) =>
            {
                item.ShouldEqual(this);
                m.ShouldEqual(Metadata);
                ++executed;
                return canExecuteValue;
            };
            var component = Add<object>(Command, execute, canExecute, allowMultipleExecution);
            component.CanExecute(Command, this, Metadata).ShouldEqual(canExecuteValue);
            executed.ShouldEqual(1);

            canExecuteValue = true;
            component.CanExecute(Command, this, Metadata).ShouldEqual(canExecuteValue);
            executed.ShouldEqual(2);
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(false, false)]
        public async Task ShouldSupportFuncBoolTaskWithObject(bool result, bool allowMultipleExecution)
        {
            var executed = 0;
            var tcs = new TaskCompletionSource<bool>();
            Func<object?, CancellationToken, IReadOnlyMetadataContext?, Task<bool>> execute = (item, c, m) =>
            {
                item.ShouldEqual(this);
                if (allowMultipleExecution)
                    c.ShouldEqual(DefaultCancellationToken);
                else
                    c.CanBeCanceled.ShouldBeTrue();
                m.ShouldEqual(Metadata);
                ++executed;
                return tcs.Task;
            };
            var component = Add<object>(Command, execute, null, allowMultipleExecution);
            var task = component.TryExecuteAsync(Command, this, DefaultCancellationToken, Metadata);
            executed.ShouldEqual(1);
            task.IsCompleted.ShouldBeFalse();
            tcs.SetResult(result);
            (await task).ShouldEqual(result);
            task.IsCompleted.ShouldBeTrue();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ShouldSupportFuncTask(bool allowMultipleExecution)
        {
            var executed = 0;
            var tcs = new TaskCompletionSource<object>();
            Func<CancellationToken, IReadOnlyMetadataContext?, Task> execute = (c, m) =>
            {
                ++executed;
                if (allowMultipleExecution)
                    c.ShouldEqual(DefaultCancellationToken);
                else
                    c.CanBeCanceled.ShouldBeTrue();
                m.ShouldEqual(Metadata);
                return tcs.Task;
            };
            var component = Add<object>(Command, execute, null, allowMultipleExecution);
            var task = component.TryExecuteAsync(Command, null, DefaultCancellationToken, Metadata);
            executed.ShouldEqual(1);
            task.IsCompleted.ShouldBeFalse();
            tcs.SetResult(this);
            (await task).ShouldBeTrue();
            task.IsCompleted.ShouldBeTrue();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ShouldSupportFuncTaskWithObject(bool allowMultipleExecution)
        {
            var executed = 0;
            var tcs = new TaskCompletionSource<object>();
            Func<object?, CancellationToken, IReadOnlyMetadataContext?, Task> execute = (item, c, m) =>
            {
                item.ShouldEqual(this);
                if (allowMultipleExecution)
                    c.ShouldEqual(DefaultCancellationToken);
                else
                    c.CanBeCanceled.ShouldBeTrue();
                m.ShouldEqual(Metadata);
                ++executed;
                return tcs.Task;
            };
            var component = Add<object>(Command, execute, null, allowMultipleExecution);
            var task = component.TryExecuteAsync(Command, this, DefaultCancellationToken, Metadata);
            executed.ShouldEqual(1);
            task.IsCompleted.ShouldBeFalse();
            tcs.SetResult(this);
            (await task).ShouldBeTrue();
            task.IsCompleted.ShouldBeTrue();
        }

        private static DelegateCommandExecutor<T> Add<T>(ICompositeCommand command, Delegate execute, Delegate? canExecute, bool allowMultipleExecution)
        {
            var component = new DelegateCommandExecutor<T>(execute, canExecute, allowMultipleExecution);
            command.AddComponent(component);
            return component;
        }
    }
}