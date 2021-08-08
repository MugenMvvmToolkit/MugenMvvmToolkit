using System.Linq;
using System.Threading.Tasks;
using MugenMvvm.Commands;
using MugenMvvm.Commands.Components;
using MugenMvvm.Extensions;
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

            cmd1.GetComponents<SynchronizationCommandExecutorDecorator>().Single().ShouldEqual(cmd2.GetComponents<SynchronizationCommandExecutorDecorator>().Single());
            cmd1.GetComponents<SynchronizationCommandExecutorDecorator>().Single().ShouldEqual(cmd3.GetComponents<SynchronizationCommandExecutorDecorator>().Single());
            cmd1.GetComponents<SynchronizationCommandExecutorDecorator>().Single().ShouldEqual(cmd4.GetComponents<SynchronizationCommandExecutorDecorator>().Single());
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ShouldCancelPreviousTokenForceExecute(bool bidirectional)
        {
            var command1 = CompositeCommand.CreateFromTask(this, (c, _) =>
            {
                var tcs = new TaskCompletionSource<object>();
                c.Register(() => tcs.TrySetCanceled(c));
                return tcs.Task;
            }, commandManager: CommandManager);
            var command2 = CompositeCommand.CreateFromTask(this, (c, _) =>
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
            var executed = 0;
            var tcs = new TaskCompletionSource<bool>();
            var command1 = CompositeCommand.CreateFromTask(this, (c, m) =>
            {
                c.CanBeCanceled.ShouldBeTrue();
                m.ShouldEqual(Metadata);
                ++executed;
                return tcs.Task;
            }, commandManager: CommandManager);
            var command2 = CompositeCommand.Create(this, m =>
            {
                m.ShouldEqual(Metadata);
                ++executed;
            }, commandManager: CommandManager);

            command1.SynchronizeWith(command2, bidirectional);
            var task = command1.ExecuteAsync(null, DefaultCancellationToken, Metadata);
            executed.ShouldEqual(1);
            command2.ExecuteAsync(null, DefaultCancellationToken, Metadata);
            executed.ShouldEqual(bidirectional ? 1 : 2);

            tcs.SetResult(default);
            (await task).ShouldEqual(default);
            task.IsCompleted.ShouldBeTrue();

            executed.ShouldEqual(bidirectional ? 1 : 2);
            command2.ExecuteAsync(null, DefaultCancellationToken, Metadata);
            executed.ShouldEqual(bidirectional ? 2 : 3);
        }
    }
}