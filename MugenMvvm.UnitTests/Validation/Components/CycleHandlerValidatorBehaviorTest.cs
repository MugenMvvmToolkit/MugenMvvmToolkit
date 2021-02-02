using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Extensions;
using MugenMvvm.UnitTests.Validation.Internal;
using MugenMvvm.Validation;
using MugenMvvm.Validation.Components;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Validation.Components
{
    public class CycleHandlerValidatorBehaviorTest : UnitTestBase
    {
        private readonly Validator _validator;

        public CycleHandlerValidatorBehaviorTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _validator = new Validator(null, ComponentCollectionManager);
            _validator.AddComponent(new CycleHandlerValidatorBehavior());
        }

        [Fact]
        public void ValidateAsyncShouldHandleCycles()
        {
            var expectedMember = "test";
            var invokeCount = 0;

            var tcs = new TaskCompletionSource<object>();
            _validator.AddComponent(new TestValidationHandlerComponent(_validator)
            {
                TryValidateAsync = (s, token, m) =>
                {
                    ++invokeCount;
                    _validator.ValidateAsync(expectedMember, token, m).IsCompleted.ShouldBeTrue();
                    return tcs.Task;
                }
            });

            var task = _validator.ValidateAsync(expectedMember, CancellationToken.None);
            task!.IsCompleted.ShouldBeFalse();
            invokeCount.ShouldEqual(1);
        }
    }
}