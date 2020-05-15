using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Extensions;
using MugenMvvm.UnitTest.Validation.Internal;
using MugenMvvm.Validation;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Validation.Components
{
    public class ValidatorComponentBaseTest : UnitTestBase
    {
        #region Methods

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ValidateAsyncShouldUpdateErrors(bool isAsync)
        {
            var invokeCount = 0;
            var expectedMember = "test";
            var tcs = new TaskCompletionSource<ValidationResult>();
            var result = new ValidationResult(new Dictionary<string, IReadOnlyList<object>?>
            {
                {expectedMember, new[] {expectedMember}}
            });
            var component = new TestValidatorComponentBase<object>(this, isAsync)
            {
                GetErrorsAsyncDelegate = (s, token, meta) =>
                {
                    ++invokeCount;
                    s.ShouldEqual(expectedMember);
                    meta.ShouldEqual(DefaultMetadata);
                    return new ValueTask<ValidationResult>(tcs.Task);
                }
            };
            var validator = new Validator();
            validator.AddComponent(component);
            if (!isAsync)
                tcs.SetResult(result);

            var task = component.ValidateAsync(expectedMember, CancellationToken.None, DefaultMetadata);
            if (isAsync)
            {
                task.IsCompleted.ShouldBeFalse();
                tcs.SetResult(result);
                task.IsCompleted.ShouldBeTrue();
            }

            component.GetErrors(expectedMember).Single().ShouldEqual(expectedMember);
            var errors = component.GetErrors();
            errors.Count.ShouldEqual(1);
            errors[expectedMember].Single().ShouldEqual(expectedMember);
        }

        [Fact]
        public void ValidateAsyncShouldUseCancellationToken()
        {
            var expectedMember = "test";
            var tcs = new TaskCompletionSource<ValidationResult>();
            var component = new TestValidatorComponentBase<object>(this, true)
            {
                GetErrorsAsyncDelegate = (s, token, _) =>
                {
                    token.Register(() => tcs.SetCanceled());
                    return new ValueTask<ValidationResult>(tcs.Task);
                }
            };
            var validator = new Validator();
            validator.AddComponent(component);

            var cts = new CancellationTokenSource();
            var task = component.ValidateAsync(expectedMember, cts.Token);
            task.IsCompleted.ShouldBeFalse();

            cts.Cancel();
            task.IsCanceled.ShouldBeTrue();
        }

        [Fact]
        public void ValidateAsyncShouldCancelPreviousValidation()
        {
            var expectedMember = "test";
            var tcs = new TaskCompletionSource<ValidationResult>();
            var component = new TestValidatorComponentBase<object>(this, true)
            {
                GetErrorsAsyncDelegate = (s, token, _) =>
                {
                    token.Register(() => tcs.SetCanceled());
                    return new ValueTask<ValidationResult>(tcs.Task);
                }
            };
            var validator = new Validator();
            validator.AddComponent(component);

            var task = component.ValidateAsync(expectedMember, CancellationToken.None);
            task.IsCompleted.ShouldBeFalse();

            component.ValidateAsync(expectedMember, CancellationToken.None);
            task.IsCanceled.ShouldBeTrue();
        }

        [Fact]
        public void ValidateAsyncShouldBeCanceledDispose()
        {
            var expectedMember = "test";
            var tcs = new TaskCompletionSource<ValidationResult>();
            var component = new TestValidatorComponentBase<object>(this, true)
            {
                GetErrorsAsyncDelegate = (s, token, _) =>
                {
                    token.Register(() => tcs.SetCanceled());
                    return new ValueTask<ValidationResult>(tcs.Task);
                }
            };
            var validator = new Validator();
            validator.AddComponent(component);

            var task = component.ValidateAsync(expectedMember, CancellationToken.None);
            task.IsCompleted.ShouldBeFalse();

            component.Dispose();
            task.IsCanceled.ShouldBeTrue();
        }

        [Fact]
        public void ValidateAsyncShouldHandleCycles()
        {
            var expectedMember = "test";
            var invokeCount = 0;
            var tcs = new TaskCompletionSource<ValidationResult>();
            var component = new TestValidatorComponentBase<object>(this, true);
            component.GetErrorsAsyncDelegate = (s, token, _) =>
            {
                ++invokeCount;
                component.ValidateAsync(expectedMember).IsCompleted.ShouldBeTrue();
                return new ValueTask<ValidationResult>(tcs.Task);
            };
            var validator = new Validator();
            validator.AddComponent(component);

            var task = component.ValidateAsync(expectedMember, CancellationToken.None);
            task.IsCompleted.ShouldBeFalse();
            invokeCount.ShouldEqual(1);
        }


        [Fact]
        public void HasErrorsShouldReturnTrueAsync()
        {
            var expectedMember = "test";
            var tcs = new TaskCompletionSource<ValidationResult>();
            var component = new TestValidatorComponentBase<object>(this, true)
            {
                GetErrorsAsyncDelegate = (s, token, _) => new ValueTask<ValidationResult>(tcs.Task)
            };
            var validator = new Validator();
            validator.AddComponent(component);

            component.HasErrors.ShouldBeFalse();
            component.ValidateAsync(expectedMember, CancellationToken.None);
            component.HasErrors.ShouldBeTrue();
            tcs.SetResult(default);
            component.HasErrors.ShouldBeFalse();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void GetErrorsShouldReturnErrors(int count)
        {
            var component = new TestValidatorComponentBase<object>(this, false)
            {
                GetErrorsAsyncDelegate = (s, token, _) => new ValueTask<ValidationResult>(ValidationResult.SingleResult(s, s))
            };
            component.HasErrors.ShouldBeFalse();

            var validator = new Validator();
            validator.AddComponent(component);

            var members = new List<string>();
            for (var i = 0; i < count; i++)
            {
                var member = i.ToString();
                members.Add(member);
                component.ValidateAsync(member);
            }

            component.HasErrors.ShouldBeTrue();
            foreach (var member in members)
                component.GetErrors(member).Single().ShouldEqual(member);

            component.GetErrors(string.Empty).SequenceEqual(members).ShouldBeTrue();
            var errors = component.GetErrors();
            errors.Count.ShouldEqual(count);
            foreach (var member in members)
                errors[member].Single().ShouldEqual(member);

            for (var i = 0; i < count; i++)
            {
                component.ClearErrors(members[0]);
                members.RemoveAt(0);
                component.GetErrors(string.Empty).SequenceEqual(members).ShouldBeTrue();
            }

            component.HasErrors.ShouldBeFalse();

            for (var i = 0; i < count; i++)
            {
                var member = i.ToString();
                members.Add(member);
                component.ValidateAsync(member);
            }

            component.HasErrors.ShouldBeTrue();
            component.ClearErrors();
            component.HasErrors.ShouldBeFalse();
            component.GetErrors(string.Empty).ShouldBeEmpty();
            component.GetErrors().ShouldBeEmpty();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void ValidateAsyncShouldUpdateAllErrorsEmptyString(int count)
        {
            var component = new TestValidatorComponentBase<object>(this, false)
            {
                GetErrorsAsyncDelegate = (s, token, _) =>
                {
                    if (string.IsNullOrEmpty(s))
                        return new ValueTask<ValidationResult>(ValidationResult.NoErrors);
                    return new ValueTask<ValidationResult>(ValidationResult.SingleResult(s, s));
                }
            };
            component.HasErrors.ShouldBeFalse();
            var validator = new Validator();
            validator.AddComponent(component);

            for (var i = 0; i < count; i++)
            {
                var member = i.ToString();
                component.ValidateAsync(member);
            }

            component.ValidateAsync(string.Empty);
            component.HasErrors.ShouldBeFalse();
            component.GetErrors(string.Empty).ShouldBeEmpty();
            component.GetErrors().ShouldBeEmpty();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void ValidateAsyncShouldNotifyListeners1(int count)
        {
            var invokeCount = 0;
            var expectedMember = "test";
            var component = new TestValidatorComponentBase<object>(this, false)
            {
                GetErrorsAsyncDelegate = (s, token, meta) => new ValueTask<ValidationResult>(ValidationResult.SingleResult(s, s))
            };
            var validator = new Validator();
            validator.AddComponent(component);

            for (var i = 0; i < count; i++)
            {
                var listener = new TestValidatorListener
                {
                    OnErrorsChanged = (v, instance, s, arg3) =>
                    {
                        ++invokeCount;
                        v.ShouldEqual(validator);
                        instance.ShouldEqual(this);
                        s.ShouldEqual(expectedMember);
                        arg3.ShouldEqual(DefaultMetadata);
                    }
                };
                validator.AddComponent(listener);
            }

            component.ValidateAsync(expectedMember, CancellationToken.None, DefaultMetadata);
            invokeCount.ShouldEqual(count);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void ValidateAsyncShouldNotifyListeners2(int count)
        {
            var invokeCount = 0;
            var expectedMember = "test";
            Task? expectedTask = null;
            var component = new TestValidatorComponentBase<object>(this, true)
            {
                GetErrorsAsyncDelegate = (s, token, meta) => new ValueTask<ValidationResult>(new TaskCompletionSource<ValidationResult>().Task)
            };
            var validator = new Validator();
            validator.AddComponent(component);

            for (var i = 0; i < count; i++)
            {
                var listener = new TestValidatorListener
                {
                    OnAsyncValidation = (v, instance, s, task, context) =>
                    {
                        ++invokeCount;
                        expectedTask = task;
                        v.ShouldEqual(validator);
                        instance.ShouldEqual(this);
                        s.ShouldEqual(expectedMember);
                        context.ShouldEqual(DefaultMetadata);
                    }
                };
                validator.AddComponent(listener);
            }

            var validateAsync = component.ValidateAsync(expectedMember, CancellationToken.None, DefaultMetadata);
            invokeCount.ShouldEqual(count);
            validateAsync.ShouldEqual(expectedTask);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void ClearShouldNotifyListeners1(int count)
        {
            var invokeCount = 0;
            var expectedMember = "test";
            var component = new TestValidatorComponentBase<object>(this, false)
            {
                GetErrorsAsyncDelegate = (s, token, meta) => new ValueTask<ValidationResult>(ValidationResult.SingleResult(s, s))
            };
            var validator = new Validator();
            validator.AddComponent(component);

            component.ValidateAsync(expectedMember, CancellationToken.None, DefaultMetadata);

            for (var i = 0; i < count; i++)
            {
                var listener = new TestValidatorListener
                {
                    OnErrorsChanged = (v, instance, s, arg3) =>
                    {
                        ++invokeCount;
                        v.ShouldEqual(validator);
                        instance.ShouldEqual(this);
                        s.ShouldEqual(expectedMember);
                        arg3.ShouldEqual(DefaultMetadata);
                    }
                };
                validator.AddComponent(listener);
            }

            component.ClearErrors(expectedMember, DefaultMetadata);
            invokeCount.ShouldEqual(count);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void ClearShouldNotifyListeners2(int count)
        {
            var invokeCount = 0;
            var expectedMember = "test";
            var component = new TestValidatorComponentBase<object>(this, false)
            {
                GetErrorsAsyncDelegate = (s, token, meta) => new ValueTask<ValidationResult>(ValidationResult.SingleResult(s, s))
            };
            var validator = new Validator();
            validator.AddComponent(component);
            component.ValidateAsync(expectedMember, CancellationToken.None, DefaultMetadata);

            for (var i = 0; i < count; i++)
            {
                var listener = new TestValidatorListener
                {
                    OnErrorsChanged = (v, instance, s, arg3) =>
                    {
                        ++invokeCount;
                        v.ShouldEqual(validator);
                        instance.ShouldEqual(this);
                        s.ShouldEqual(expectedMember);
                        arg3.ShouldEqual(DefaultMetadata);
                    }
                };
                validator.AddComponent(listener);
            }

            component.ClearErrors(null, DefaultMetadata);
            invokeCount.ShouldEqual(count);
        }

        #endregion
    }
}