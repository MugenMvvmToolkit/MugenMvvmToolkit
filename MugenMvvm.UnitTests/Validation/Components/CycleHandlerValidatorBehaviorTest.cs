using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Extensions;
using MugenMvvm.Tests.Validation;
using MugenMvvm.Validation.Components;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Validation.Components
{
    public class CycleHandlerValidatorBehaviorTest : UnitTestBase
    {
        public CycleHandlerValidatorBehaviorTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            Validator.AddComponent(new CycleHandlerValidatorBehavior());
        }

        [Fact]
        public void ValidateAsyncShouldHandleCycles()
        {
            var expectedMember = "test";
            var invokeCount = 0;

            var tcs = new TaskCompletionSource<object>();
            Validator.AddComponent(new TestValidationHandlerComponent
            {
                TryValidateAsync = (v, s, token, m) =>
                {
                    ++invokeCount;
                    s.ShouldEqual(expectedMember);
                    v.ShouldEqual(Validator);
                    Validator.ValidateAsync(expectedMember, token, m).IsCompleted.ShouldBeTrue();
                    return tcs.Task;
                }
            });

            var task = Validator.ValidateAsync(expectedMember, CancellationToken.None);
            task!.IsCompleted.ShouldBeFalse();
            invokeCount.ShouldEqual(1);
        }
    }
}