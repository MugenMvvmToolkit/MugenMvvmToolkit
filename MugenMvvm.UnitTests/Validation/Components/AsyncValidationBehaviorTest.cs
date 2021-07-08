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
    public class AsyncValidationBehaviorTest : UnitTestBase
    {
        public AsyncValidationBehaviorTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            Validator.AddComponent(new AsyncValidationBehavior());
        }

        [Fact]
        public async Task HasErrorsShouldReturnTrueAsync()
        {
            var expectedMember = "test";
            var tcs = new TaskCompletionSource<object?>();
            Validator.AddComponent(new TestValidationHandlerComponent
            {
                TryValidateAsync = (v, _, _, _) =>
                {
                    v.ShouldEqual(Validator);
                    return tcs.Task;
                }
            });

            Validator.HasErrors().ShouldBeFalse();
            var task = Validator.ValidateAsync(expectedMember, CancellationToken.None);
            Validator.HasErrors().ShouldBeTrue();
            Validator.HasErrors(expectedMember).ShouldBeTrue();
            tcs.SetResult(null);
            await task;

            Validator.HasErrors().ShouldBeFalse();
            Validator.HasErrors(expectedMember).ShouldBeFalse();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public async Task ShouldNotifyListeners(int count)
        {
            var expectedMember = "test";
            var invokeCount = 0;
            var tcs = new TaskCompletionSource<object?>();
            Validator.AddComponent(new TestValidationHandlerComponent
            {
                TryValidateAsync = (v, _, _, _) =>
                {
                    v.ShouldEqual(Validator);
                    return tcs.Task;
                }
            });

            for (var i = 0; i < count; i++)
            {
                Validator.AddComponent(new TestAsyncValidationListener
                {
                    OnAsyncValidation = (v, m, t, meta) =>
                    {
                        ++invokeCount;
                        v.ShouldEqual(Validator);
                        m.ShouldEqual(expectedMember);
                        t.ShouldEqual(tcs.Task);
                        meta.ShouldEqual(DefaultMetadata);
                    }
                });
            }

            invokeCount.ShouldEqual(0);
            var task = Validator.ValidateAsync(expectedMember, CancellationToken.None, DefaultMetadata);
            invokeCount.ShouldEqual(count);
            tcs.TrySetResult(null);
            await task;
        }

        [Fact]
        public async Task ValidateAsyncShouldCancelPreviousValidation()
        {
            var expectedMember = "test";
            var tcs = new TaskCompletionSource<object>();
            Validator.AddComponent(new TestValidationHandlerComponent
            {
                TryValidateAsync = (v, s, token, arg3) =>
                {
                    v.ShouldEqual(Validator);
                    token.Register(() => tcs.SetCanceled());
                    return tcs.Task;
                }
            });

            var task = Validator.ValidateAsync(expectedMember, CancellationToken.None)!;
            task.IsCompleted.ShouldBeFalse();

#pragma warning disable 4014
            Validator.ValidateAsync(expectedMember, CancellationToken.None);
#pragma warning restore 4014
            await task.WaitSafeAsync();
            task.IsCanceled.ShouldBeTrue();
        }
    }
}