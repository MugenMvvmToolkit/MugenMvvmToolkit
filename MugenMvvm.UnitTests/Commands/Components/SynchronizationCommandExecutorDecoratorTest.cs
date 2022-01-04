﻿using System.Linq;
using System.Threading.Tasks;
using MugenMvvm.Commands;
using MugenMvvm.Commands.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Metadata;
using Should;
using Xunit;
using Xunit.Abstractions;

#pragma warning disable 4014

namespace MugenMvvm.UnitTests.Commands.Components
{
    public class SynchronizationCommandExecutorDecoratorTest : UnitTestBase
    {
        public SynchronizationCommandExecutorDecoratorTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
        }

        [Fact]
        public void ShouldSynchronizeMultipleCommands()
        {
            var cmd1 = CompositeCommand.Create(this, commandManager: CommandManager);
            var cmd2 = CompositeCommand.Create(this, commandManager: CommandManager);
            var cmd3 = CompositeCommand.Create(this, commandManager: CommandManager);
            var cmd4 = CompositeCommand.Create(this, commandManager: CommandManager);

            cmd1.SynchronizeWith(cmd2);
            cmd3.SynchronizeWith(cmd4);
            cmd4.SynchronizeWith(cmd1);

            var commandExecutorDecorator = cmd1.GetComponents<SynchronizationCommandExecutorDecorator>().Single();
            commandExecutorDecorator.ShouldEqual(cmd2.GetComponents<SynchronizationCommandExecutorDecorator>().Single());
            commandExecutorDecorator.ShouldEqual(cmd3.GetComponents<SynchronizationCommandExecutorDecorator>().Single());
            commandExecutorDecorator.ShouldEqual(cmd4.GetComponents<SynchronizationCommandExecutorDecorator>().Single());

            cmd1.GetComponents<SynchronizationCommandExecutorDecorator.CommandExecutorInterceptor>().Single().Synchronizer.ShouldEqual(commandExecutorDecorator);
            cmd2.GetComponents<SynchronizationCommandExecutorDecorator.CommandExecutorInterceptor>().Single().Synchronizer.ShouldEqual(commandExecutorDecorator);
            cmd3.GetComponents<SynchronizationCommandExecutorDecorator.CommandExecutorInterceptor>().Single().Synchronizer.ShouldEqual(commandExecutorDecorator);
            cmd4.GetComponents<SynchronizationCommandExecutorDecorator.CommandExecutorInterceptor>().Single().Synchronizer.ShouldEqual(commandExecutorDecorator);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ShouldCancelPreviousTokenForceExecute(bool bidirectional)
        {
            var command1 = CompositeCommand.Create(this, (c, _) =>
            {
                var tcs = new TaskCompletionSource<object>();
                c.Register(() => tcs.TrySetCanceled(c));
                return tcs.Task;
            }, commandManager: CommandManager);
            var command2 = CompositeCommand.Create(this, (c, _) =>
            {
                var tcs = new TaskCompletionSource<object>();
                c.Register(() => tcs.TrySetCanceled(c));
                return tcs.Task;
            }, commandManager: CommandManager);

            command1.SynchronizeWith(command2, bidirectional);
            var task1 = command1.ExecuteAsync();
            var task2 = bidirectional ? command2.ForceExecuteAsync() : command2.ExecuteAsync();
            await task1.WaitSafeAsync();
            task1.IsCanceled.ShouldBeTrue();

            task1 = bidirectional ? command1.ForceExecuteAsync() : command1.ExecuteAsync();
            if (bidirectional)
            {
                await task2.WaitSafeAsync();
                task2.IsCanceled.ShouldBeTrue();
            }
            else
            {
                (await task1).ShouldBeFalse();
                command1.CanExecute(null).ShouldBeFalse();
                task2.IsCompleted.ShouldBeFalse();
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task SynchronizeExecutionShouldSynchronizeExecutionBetweenCommands(bool bidirectional)
        {
            var executed1 = 0;
            var executed2 = 0;
            var tcs = new TaskCompletionSource<bool>();
            var command1 = CompositeCommand.Create(this, (c, m) =>
            {
                c.CanBeCanceled.ShouldBeTrue();
                m!.Get(CommandMetadata.SynchronizerToken).ShouldNotBeNull();
                ++executed1;
                return tcs.Task;
            }, commandManager: CommandManager);
            var command2 = CompositeCommand.Create(this, m =>
            {
                m!.Get(CommandMetadata.SynchronizerToken).ShouldNotBeNull();
                ++executed2;
            }, commandManager: CommandManager);

            command1.SynchronizeWith(command2, bidirectional);
            command1.IsExecuting().ShouldBeFalse();
            command2.IsExecuting().ShouldBeFalse();

            var task = command1.ExecuteAsync(null, DefaultCancellationToken);
            command1.IsExecuting().ShouldBeTrue();
            command2.IsExecuting().ShouldBeFalse();

            executed1.ShouldEqual(1);
            command2.ExecuteAsync(null, DefaultCancellationToken);
            executed2.ShouldEqual(bidirectional ? 0 : 1);

            tcs.SetResult(default);
            await task.WaitSafeAsync();
            task.IsCompleted.ShouldBeTrue();
            command1.IsExecuting().ShouldBeFalse();
            command2.IsExecuting().ShouldBeFalse();

            executed1.ShouldEqual(1);
            executed2.ShouldEqual(bidirectional ? 0 : 1);
            command2.ExecuteAsync(null, DefaultCancellationToken);
            executed1.ShouldEqual(1);
            executed2.ShouldEqual(bidirectional ? 1 : 2);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task SynchronizeExecutionShouldHandleRecursiveExecution(bool includeMetadata)
        {
            var executed1 = 0;
            var executed2 = 0;
            var command2 = CompositeCommand.Create(this, (c, m) =>
            {
                c.CanBeCanceled.ShouldBeTrue();
                m!.Get(CommandMetadata.SynchronizerToken).ShouldNotBeNull();
                ++executed2;
                return Task.CompletedTask;
            }, commandManager: CommandManager);
            var command1 = CompositeCommand.Create(this, (c, m) =>
            {
                c.CanBeCanceled.ShouldBeTrue();
                m!.Get(CommandMetadata.SynchronizerToken).ShouldNotBeNull();
                ++executed1;
                return command2.ExecuteAsync(null, includeMetadata ? c : default, includeMetadata ? m : null);
            }, commandManager: CommandManager);
            command1.SynchronizeWith(command2);

            await command1.ExecuteAsync();
            executed1.ShouldEqual(1);
            executed2.ShouldEqual(includeMetadata ? 1 : 0);
        }
    }
}