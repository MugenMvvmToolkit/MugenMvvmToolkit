using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Extensions;
using MugenMvvm.UnitTests.Validation.Internal;
using MugenMvvm.Validation;
using MugenMvvm.Validation.Components;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Validation.Components
{
    public class CycleHandlerValidatorBehaviorTest : UnitTestBase
    {
        [Fact]
        public void ValidateAsyncShouldHandleCycles()
        {
            var expectedMember = "test";
            var invokeCount = 0;
            var validator = new Validator();
            var tcs = new TaskCompletionSource<object>();
            validator.AddComponent(new TestValidationHandlerComponent(validator)
            {
                TryValidateAsync = (s, token, m) =>
                {
                    ++invokeCount;
                    validator.ValidateAsync(expectedMember, token, m).IsCompleted.ShouldBeTrue();
                    return tcs.Task;
                }
            });
            validator.AddComponent(new CycleHandlerValidatorBehavior());

            var task = validator.ValidateAsync(expectedMember, CancellationToken.None);
            task!.IsCompleted.ShouldBeFalse();
            invokeCount.ShouldEqual(1);
        }
    }
}