using System.Threading.Tasks;
using MugenMvvm.Commands.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Internal;
using MugenMvvm.Tests.Commands;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Commands.Components
{
    public class CanExecuteCommandConditionTest : UnitTestBase
    {
        public CanExecuteCommandConditionTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            Command.AddComponent(new CanExecuteCommandCondition());
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public async Task ShouldCheckCanExecuteBeforeExecute(int componentCount)
        {
            var canExecuteCount = 0;
            var invokeCount = 0;
            var canExecute = false;
            for (var i = 0; i < componentCount; i++)
            {
                var isLast = i == componentCount - 1;
                Command.AddComponent(new TestCommandConditionComponent
                {
                    CanExecute = (c, item, m) =>
                    {
                        c.ShouldEqual(Command);
                        item.ShouldEqual(this);
                        m.ShouldEqual(Metadata);
                        ++canExecuteCount;
                        if (isLast)
                            return canExecute;
                        return true;
                    },
                    Priority = -i
                });

                Command.AddComponent(new TestCommandExecutorComponent
                {
                    ExecuteAsync = (cmd, p, c, m) =>
                    {
                        cmd.ShouldEqual(Command);
                        p.ShouldEqual(this);
                        c.ShouldEqual(DefaultCancellationToken);
                        m.ShouldEqual(Metadata);
                        ++invokeCount;
                        return Default.FalseTask;
                    },
                    Priority = -i
                });
            }


            await Command.ExecuteAsync(this, DefaultCancellationToken, Metadata);
            canExecuteCount.ShouldEqual(componentCount);
            invokeCount.ShouldEqual(0);

            canExecuteCount = 0;
            canExecute = true;
            await Command.ExecuteAsync(this, DefaultCancellationToken, Metadata);
            canExecuteCount.ShouldEqual(componentCount);
            invokeCount.ShouldEqual(componentCount);
        }
    }
}