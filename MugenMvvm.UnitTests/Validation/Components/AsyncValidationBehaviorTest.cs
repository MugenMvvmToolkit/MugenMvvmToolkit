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
    public class AsyncValidationBehaviorTest : UnitTestBase
    {
        private readonly Validator _validator;

        public AsyncValidationBehaviorTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _validator = new Validator(null, ComponentCollectionManager);
            _validator.AddComponent(new AsyncValidationBehavior());
        }

        [Fact]
        public async Task HasErrorsShouldReturnTrueAsync()
        {
            var expectedMember = "test";
            var tcs = new TaskCompletionSource<object?>();
            _validator.AddComponent(new TestValidationHandlerComponent(_validator)
            {
                TryValidateAsync = (s, token, arg3) => tcs.Task
            });

            _validator.HasErrors().ShouldBeFalse();
            var task = _validator.ValidateAsync(expectedMember, CancellationToken.None);
            _validator.HasErrors().ShouldBeTrue();
            _validator.HasErrors(expectedMember).ShouldBeTrue();
            tcs.SetResult(null);
            await task;

            _validator.HasErrors().ShouldBeFalse();
            _validator.HasErrors(expectedMember).ShouldBeFalse();
        }

        [Fact]
        public async Task ValidateAsyncShouldCancelPreviousValidation()
        {
            var expectedMember = "test";
            var tcs = new TaskCompletionSource<object>();
            _validator.AddComponent(new TestValidationHandlerComponent(_validator)
            {
                TryValidateAsync = (s, token, arg3) =>
                {
                    token.Register(() => tcs.SetCanceled());
                    return tcs.Task;
                }
            });

            var task = _validator.ValidateAsync(expectedMember, CancellationToken.None)!;
            task.IsCompleted.ShouldBeFalse();

#pragma warning disable 4014
            _validator.ValidateAsync(expectedMember, CancellationToken.None);
#pragma warning restore 4014
            await task.WaitSafeAsync();
            task.IsCanceled.ShouldBeTrue();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public async Task ShouldNotifyListeners(int count)
        {
            var expectedMember = "test";
            var invokeCount = 0;
            var tcs = new TaskCompletionSource<object?>();
            _validator.AddComponent(new TestValidationHandlerComponent(_validator)
            {
                TryValidateAsync = (s, token, arg3) => tcs.Task
            });

            for (var i = 0; i < count; i++)
            {
                _validator.AddComponent(new TestAsyncValidationListener
                {
                    OnAsyncValidation = (v, m, t, meta) =>
                    {
                        ++invokeCount;
                        v.ShouldEqual(_validator);
                        m.ShouldEqual(expectedMember);
                        t.ShouldEqual(tcs.Task);
                        meta.ShouldEqual(DefaultMetadata);
                    }
                });
            }

            invokeCount.ShouldEqual(0);
            var task = _validator.ValidateAsync(expectedMember, CancellationToken.None, DefaultMetadata);
            invokeCount.ShouldEqual(count);
            tcs.TrySetResult(null);
            await task;
        }
    }
}