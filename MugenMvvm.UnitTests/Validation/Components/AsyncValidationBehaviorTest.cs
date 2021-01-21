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
    public class AsyncValidationBehaviorTest : UnitTestBase
    {
        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public async Task ShouldNotifyListeners(int count)
        {
            var expectedMember = "test";
            int invokeCount = 0;
            var tcs = new TaskCompletionSource<object?>();
            var validator = new Validator();
            validator.AddComponent(new TestValidationHandlerComponent(validator)
            {
                TryValidateAsync = (s, token, arg3) => tcs.Task
            });
            validator.AddComponent(new AsyncValidationBehavior());
            for (int i = 0; i < count; i++)
            {
                validator.AddComponent(new TestAsyncValidationListener
                {
                    OnAsyncValidation = (v, m, t, meta) =>
                    {
                        ++invokeCount;
                        v.ShouldEqual(validator);
                        m.ShouldEqual(expectedMember);
                        t.ShouldEqual(tcs.Task);
                        meta.ShouldEqual(DefaultMetadata);
                    }
                });
            }

            invokeCount.ShouldEqual(0);
            var task = validator.ValidateAsync(expectedMember, CancellationToken.None, DefaultMetadata);
            invokeCount.ShouldEqual(count);
            tcs.TrySetResult(null);
            await task;
        }

        [Fact]
        public async Task HasErrorsShouldReturnTrueAsync()
        {
            var expectedMember = "test";
            var tcs = new TaskCompletionSource<object?>();
            var validator = new Validator();
            validator.AddComponent(new TestValidationHandlerComponent(validator)
            {
                TryValidateAsync = (s, token, arg3) => tcs.Task
            });
            validator.AddComponent(new AsyncValidationBehavior());

            validator.HasErrors().ShouldBeFalse();
            var task = validator.ValidateAsync(expectedMember, CancellationToken.None);
            validator.HasErrors().ShouldBeTrue();
            validator.HasErrors(expectedMember).ShouldBeTrue();
            tcs.SetResult(null);
            await task;

            validator.HasErrors().ShouldBeFalse();
            validator.HasErrors(expectedMember).ShouldBeFalse();
        }

        [Fact]
        public async Task ValidateAsyncShouldCancelPreviousValidation()
        {
            var expectedMember = "test";
            var tcs = new TaskCompletionSource<object>();
            var validator = new Validator();
            validator.AddComponent(new TestValidationHandlerComponent(validator)
            {
                TryValidateAsync = (s, token, arg3) =>
                {
                    token.Register(() => tcs.SetCanceled());
                    return tcs.Task;
                }
            });
            validator.AddComponent(new AsyncValidationBehavior());

            var task = validator.ValidateAsync(expectedMember, CancellationToken.None)!;
            task.IsCompleted.ShouldBeFalse();

#pragma warning disable 4014
            validator.ValidateAsync(expectedMember, CancellationToken.None);
#pragma warning restore 4014
            await task.WaitSafeAsync();
            task.IsCanceled.ShouldBeTrue();
        }
    }
}