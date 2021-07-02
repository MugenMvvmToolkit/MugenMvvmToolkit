﻿#pragma warning disable 4014
using System;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Commands;
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
            var component = (DelegateCommandExecutor.IDelegateCommandExecutor)DelegateCommandExecutor.Add<object>(Command, execute, null, allowMultipleExecution);
            Command.GetComponents<IDisposableComponent<ICompositeCommand>>().Dispose(Command, null);

            component.CanExecute(Command, null, DefaultMetadata).ShouldBeFalse();
            await component.TryExecuteAsync(Command, null, DefaultCancellationToken, DefaultMetadata);
            executed.ShouldEqual(0);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ShouldNotifyCanExecuteChangedAllowMultipleExecution(bool allowMultipleExecution)
        {
            var executed = 0;
            var tcs = new TaskCompletionSource<object>();
            Func<CancellationToken, IReadOnlyMetadataContext?, Task> execute = (c, m) => tcs.Task;

            var listener = new TestCommandEventHandlerComponent { RaiseCanExecuteChanged = c => ++executed };
            Command.AddComponent(listener);

            var component = (DelegateCommandExecutor.IDelegateCommandExecutor)DelegateCommandExecutor.Add<object>(Command, execute, null, allowMultipleExecution);
            executed.ShouldEqual(1);
            executed = 0;
            var task = Command.ExecuteAsync(this, DefaultCancellationToken, DefaultMetadata);
            executed.ShouldEqual(allowMultipleExecution ? 0 : 1);

            component.CanExecute(Command, null, DefaultMetadata).ShouldEqual(allowMultipleExecution);
            tcs.SetResult(this);
            await task;
            executed.ShouldEqual(allowMultipleExecution ? 0 : 2);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ShouldNotThrowException(bool allowMultipleExecution)
        {
            var exception = new NotSupportedException();
            Action<IReadOnlyMetadataContext?> execute = m => throw exception;
            var component = DelegateCommandExecutor.Add<object>(Command, execute, null, allowMultipleExecution);
            var task = component.TryExecuteAsync(Command, null, default, null).AsTask();
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
                m.ShouldEqual(DefaultMetadata);
                ++executed;
            };
            var component = DelegateCommandExecutor.Add<object>(Command, execute, null, allowMultipleExecution);
            await component.TryExecuteAsync(Command, null, default, DefaultMetadata);
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
                m.ShouldEqual(DefaultMetadata);
                ++executed;
            };
            var component = DelegateCommandExecutor.Add<object>(Command, execute, null, allowMultipleExecution);
            await component.TryExecuteAsync(Command, this, default, DefaultMetadata);
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
                c.ShouldEqual(DefaultCancellationToken);
                m.ShouldEqual(DefaultMetadata);
                return tcs.Task;
            };
            var component = (DelegateCommandExecutor.IDelegateCommandExecutor)DelegateCommandExecutor.Add<object>(Command, execute, null, allowMultipleExecution);
            var task1 = component.TryExecuteAsync(Command, null, DefaultCancellationToken, DefaultMetadata);
            var task2 = component.TryExecuteAsync(Command, null, DefaultCancellationToken, DefaultMetadata);

            component.CanExecute(Command, null, DefaultMetadata).ShouldEqual(allowMultipleExecution);
            tcs.SetResult(this);
            await task1;
            await task2;
            executed.ShouldEqual(allowMultipleExecution ? 2 : 1);
            await component.TryExecuteAsync(Command, null, DefaultCancellationToken, DefaultMetadata);
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
                c.ShouldEqual(DefaultCancellationToken);
                m.ShouldEqual(context);
                return tcs.Task;
            };
            var component = (DelegateCommandExecutor.IDelegateCommandExecutor)DelegateCommandExecutor.Add<object>(Command, execute, null, allowMultipleExecution);
            var task1 = component.TryExecuteAsync(Command, null, DefaultCancellationToken, context);
            var task2 = component.TryExecuteAsync(Command, null, DefaultCancellationToken, context);

            component.CanExecute(Command, null, context).ShouldBeTrue();
            context = DefaultMetadata;
            component.CanExecute(Command, null, DefaultMetadata).ShouldEqual(allowMultipleExecution);
            tcs.SetResult(this);
            await task1;
            await task2;
            executed.ShouldEqual(2);
            await component.TryExecuteAsync(Command, null, DefaultCancellationToken, DefaultMetadata);
            executed.ShouldEqual(3);
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
                m.ShouldEqual(DefaultMetadata);
                return canExecuteValue;
            };
            var component = (DelegateCommandExecutor.IDelegateCommandExecutor)DelegateCommandExecutor.Add<object>(Command, execute, canExecute, allowMultipleExecution);
            component.CanExecute(Command, null, DefaultMetadata).ShouldEqual(canExecuteValue);
            executed.ShouldEqual(1);

            canExecuteValue = true;
            component.CanExecute(Command, null, DefaultMetadata).ShouldEqual(canExecuteValue);
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
                m.ShouldEqual(DefaultMetadata);
                ++executed;
                return canExecuteValue;
            };
            var component = (DelegateCommandExecutor.IDelegateCommandExecutor)DelegateCommandExecutor.Add<object>(Command, execute, canExecute, allowMultipleExecution);
            component.CanExecute(Command, this, DefaultMetadata).ShouldEqual(canExecuteValue);
            executed.ShouldEqual(1);

            canExecuteValue = true;
            component.CanExecute(Command, this, DefaultMetadata).ShouldEqual(canExecuteValue);
            executed.ShouldEqual(2);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ShouldSupportCommandExecutionModeCanExecuteBeforeExecute(bool allowMultipleExecution)
        {
            var executed = 0;
            var canExecuted = 0;
            Action<IReadOnlyMetadataContext?> execute = m =>
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

            var component = DelegateCommandExecutor.Add<object>(Command, execute, canExecute, allowMultipleExecution);

            await component.TryExecuteAsync(Command, null, default, DefaultMetadata);
            executed.ShouldEqual(0);
            canExecuted.ShouldEqual(1);

            canExecuteValue = true;
            await component.TryExecuteAsync(Command, null, default, DefaultMetadata);
            executed.ShouldEqual(1);
            canExecuted.ShouldEqual(2);
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
                c.ShouldEqual(DefaultCancellationToken);
                m.ShouldEqual(DefaultMetadata);
                return tcs.Task;
            };
            var component = DelegateCommandExecutor.Add<object>(Command, execute, null, allowMultipleExecution);
            var task = component.TryExecuteAsync(Command, null, DefaultCancellationToken, DefaultMetadata);
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
                c.ShouldEqual(DefaultCancellationToken);
                m.ShouldEqual(DefaultMetadata);
                ++executed;
                return tcs.Task;
            };
            var component = DelegateCommandExecutor.Add<object>(Command, execute, null, allowMultipleExecution);
            var task = component.TryExecuteAsync(Command, this, DefaultCancellationToken, DefaultMetadata);
            executed.ShouldEqual(1);
            task.IsCompleted.ShouldBeFalse();
            tcs.SetResult(this);
            (await task).ShouldBeTrue();
            task.IsCompleted.ShouldBeTrue();
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(false, false)]
        public async Task ShouldSupportFuncValueTaskWithObject(bool result, bool allowMultipleExecution)
        {
            var executed = 0;
            var tcs = new TaskCompletionSource<bool>();
            Func<object?, CancellationToken, IReadOnlyMetadataContext?, ValueTask<bool>> execute = (item, c, m) =>
            {
                item.ShouldEqual(this);
                c.ShouldEqual(DefaultCancellationToken);
                m.ShouldEqual(DefaultMetadata);
                ++executed;
                return tcs.Task.AsValueTask();
            };
            var component = DelegateCommandExecutor.Add<object>(Command, execute, null, allowMultipleExecution);
            var task = component.TryExecuteAsync(Command, this, DefaultCancellationToken, DefaultMetadata);
            executed.ShouldEqual(1);
            task.IsCompleted.ShouldBeFalse();
            tcs.SetResult(result);
            (await task).ShouldEqual(result);
            task.IsCompleted.ShouldBeTrue();
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(false, false)]
        public async Task ShouldSupportValueFuncTask(bool result, bool allowMultipleExecution)
        {
            var executed = 0;
            var tcs = new TaskCompletionSource<bool>();
            Func<CancellationToken, IReadOnlyMetadataContext?, ValueTask<bool>> execute = (c, m) =>
            {
                ++executed;
                c.ShouldEqual(DefaultCancellationToken);
                m.ShouldEqual(DefaultMetadata);
                return tcs.Task.AsValueTask();
            };
            var component = DelegateCommandExecutor.Add<object>(Command, execute, null, allowMultipleExecution);
            var task = component.TryExecuteAsync(Command, null, DefaultCancellationToken, DefaultMetadata);
            executed.ShouldEqual(1);
            task.IsCompleted.ShouldBeFalse();
            tcs.SetResult(result);
            (await task).ShouldEqual(result);
            task.IsCompleted.ShouldBeTrue();
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        public async Task SynchronizeExecutionShouldSynchronizeExecutionBetweenCommands(bool allowMultipleExecution1, bool allowMultipleExecution2)
        {
            var executed = 0;
            var tcs = new TaskCompletionSource<bool>();
            Func<CancellationToken, IReadOnlyMetadataContext?, ValueTask<bool>> execute1 = (c, m) =>
            {
                c.ShouldEqual(DefaultCancellationToken);
                m.ShouldEqual(DefaultMetadata);
                ++executed;
                return tcs.Task.AsValueTask();
            };
            Action<IReadOnlyMetadataContext?> execute2 = m =>
            {
                m.ShouldEqual(DefaultMetadata);
                ++executed;
            };

            var command2 = new CompositeCommand(null, ComponentCollectionManager);
            DelegateCommandExecutor.Add<object>(Command, execute1, null, allowMultipleExecution1);
            DelegateCommandExecutor.Add<object>(command2, execute2, null, allowMultipleExecution2);

            DelegateCommandExecutor.SynchronizeExecution(Command, command2);
            var task = Command.ExecuteAsync(null, DefaultCancellationToken, DefaultMetadata);
            executed.ShouldEqual(1);
            command2.ExecuteAsync(null, DefaultCancellationToken, DefaultMetadata);
            executed.ShouldEqual(1);

            tcs.SetResult(default);
            (await task).ShouldEqual(default);
            task.IsCompleted.ShouldBeTrue();

            executed.ShouldEqual(1);
            command2.ExecuteAsync(null, DefaultCancellationToken, DefaultMetadata);
            executed.ShouldEqual(2);
        }
    }
}