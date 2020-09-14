using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Extensions;
using MugenMvvm.UnitTests.Validation.Internal;
using MugenMvvm.Validation;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Validation.Components
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
            var result = ValidationResult.Get(new Dictionary<string, object?>
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

            var task = component.TryValidateAsync(null!, expectedMember, CancellationToken.None, DefaultMetadata)!;
            if (isAsync)
            {
                task.IsCompleted.ShouldBeFalse();
                tcs.SetResult(result);
                task.IsCompleted.ShouldBeTrue();
            }

            component.TryGetErrors(null!, expectedMember).AsList().Single().ShouldEqual(expectedMember);
            var errors = component.TryGetErrors(null!);
            errors.Count.ShouldEqual(1);
            errors[expectedMember].AsList().Single().ShouldEqual(expectedMember);
            invokeCount.ShouldEqual(1);
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
            var task = component.TryValidateAsync(null!, expectedMember, cts.Token)!;
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

            var task = component.TryValidateAsync(null!, expectedMember, CancellationToken.None)!;
            task.IsCompleted.ShouldBeFalse();

            component.TryValidateAsync(null!, expectedMember, CancellationToken.None);
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

            var task = component.TryValidateAsync(null!, expectedMember, CancellationToken.None)!;
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
                component.TryValidateAsync(null!, expectedMember).ShouldBeNull();
                return new ValueTask<ValidationResult>(tcs.Task);
            };
            var validator = new Validator();
            validator.AddComponent(component);

            var task = component.TryValidateAsync(null!, expectedMember, CancellationToken.None);
            task!.IsCompleted.ShouldBeFalse();
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

            component.HasErrors(null!).ShouldBeFalse();
            component.TryValidateAsync(null!, expectedMember, CancellationToken.None);
            component.HasErrors(null!).ShouldBeTrue();
            component.HasErrors(null!, expectedMember).ShouldBeTrue();
            tcs.SetResult(default);
            component.HasErrors(null!).ShouldBeFalse();
            component.HasErrors(null!, expectedMember).ShouldBeFalse();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void GetErrorsShouldReturnErrors(int count)
        {
            var component = new TestValidatorComponentBase<object>(this, false)
            {
                GetErrorsAsyncDelegate = (s, token, _) => new ValueTask<ValidationResult>(ValidationResult.Get(s, s))
            };
            component.HasErrors(null!).ShouldBeFalse();

            var validator = new Validator();
            validator.AddComponent(component);

            var members = new List<string>();
            for (var i = 0; i < count; i++)
            {
                var member = i.ToString();
                members.Add(member);
                component.TryValidateAsync(null!, member);
            }

            component.HasErrors(null!).ShouldBeTrue();
            foreach (var member in members)
            {
                component.TryGetErrors(null!, member).AsList().Single().ShouldEqual(member);
                component.HasErrors(null!, member).ShouldBeTrue();
            }

            component.TryGetErrors(null!, string.Empty).AsList().SequenceEqual(members).ShouldBeTrue();
            var errors = component.TryGetErrors(null!);
            errors.Count.ShouldEqual(count);
            foreach (var member in members)
                errors[member].AsList().Single().ShouldEqual(member);

            for (var i = 0; i < count; i++)
            {
                component.ClearErrors(null!, members[0]);
                members.RemoveAt(0);
                component.TryGetErrors(null!, string.Empty).AsList().SequenceEqual(members).ShouldBeTrue();
            }

            component.HasErrors(null!).ShouldBeFalse();

            for (var i = 0; i < count; i++)
            {
                var member = i.ToString();
                members.Add(member);
                component.TryValidateAsync(null!, member);
            }

            component.HasErrors(null!).ShouldBeTrue();
            component.ClearErrors(null!);
            component.HasErrors(null!).ShouldBeFalse();
            component.TryGetErrors(null!, string.Empty).AsList().ShouldBeEmpty();
            component.TryGetErrors(null!).ShouldBeEmpty();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void ValidateAsyncShouldUpdateAllErrorsEmptyString1(int count)
        {
            var component = new TestValidatorComponentBase<object>(this, false)
            {
                GetErrorsAsyncDelegate = (s, token, _) =>
                {
                    if (string.IsNullOrEmpty(s))
                        return new ValueTask<ValidationResult>(ValidationResult.NoErrors);
                    return new ValueTask<ValidationResult>(ValidationResult.Get(s, s));
                }
            };
            component.HasErrors(null!).ShouldBeFalse();
            var validator = new Validator();
            validator.AddComponent(component);

            for (var i = 0; i < count; i++)
            {
                var member = i.ToString();
                component.TryValidateAsync(null!, member);
                component.HasErrors(null!, member).ShouldBeTrue();
            }

            component.HasErrors(null!).ShouldBeTrue();
            component.TryValidateAsync(null!, string.Empty);
            component.HasErrors(null!).ShouldBeFalse();
            component.TryGetErrors(null!, string.Empty).AsList().ShouldBeEmpty();
            component.TryGetErrors(null!).ShouldBeEmpty();
        }

        [Fact]
        public void ValidateAsyncShouldUpdateAllErrorsEmptyString2()
        {
            var errors = new Dictionary<string, object?>
            {
                {"test0", new[] {"1", "2"}}
            };
            var emptyStringErrors = new Dictionary<string, object?>
            {
                {"test1", new[] {"1", "2"}}
            };
            var component = new TestValidatorComponentBase<object>(this, false)
            {
                GetErrorsAsyncDelegate = (s, token, _) =>
                {
                    if (string.IsNullOrEmpty(s))
                        return new ValueTask<ValidationResult>(ValidationResult.Get(emptyStringErrors));
                    return new ValueTask<ValidationResult>(ValidationResult.Get(errors));
                }
            };
            component.HasErrors(null!).ShouldBeFalse();
            var validator = new Validator();
            validator.AddComponent(component);

            component.TryValidateAsync(null!, "test");
            component.TryGetErrors(null!, errors.First().Key).AsList().SequenceEqual((IEnumerable<object>) errors.First().Value!).ShouldBeTrue();
            var pair = component.TryGetErrors(null!).Single();
            pair.Key.ShouldEqual(errors.First().Key);
            pair.Value.AsList().SequenceEqual((IEnumerable<object>) errors.First().Value!).ShouldBeTrue();
            component.HasErrors(null!).ShouldBeTrue();
            component.HasErrors(null!, errors.First().Key).ShouldBeTrue();

            component.TryValidateAsync(null!, string.Empty);
            component.TryGetErrors(null!, emptyStringErrors.First().Key).AsList().SequenceEqual((IEnumerable<object>) emptyStringErrors.First().Value!).ShouldBeTrue();
            pair = component.TryGetErrors(null!).Single();
            pair.Key.ShouldEqual(emptyStringErrors.First().Key);
            pair.Value.AsList().SequenceEqual((IEnumerable<object>) emptyStringErrors.First().Value!).ShouldBeTrue();
            component.HasErrors(null!).ShouldBeTrue();
            component.HasErrors(null!, emptyStringErrors.First().Key).ShouldBeTrue();
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
                GetErrorsAsyncDelegate = (s, token, meta) => new ValueTask<ValidationResult>(ValidationResult.Get(s, s))
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

            component.TryValidateAsync(null!, expectedMember, CancellationToken.None, DefaultMetadata);
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

            var validateAsync = component.TryValidateAsync(null!, expectedMember, CancellationToken.None, DefaultMetadata);
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
                GetErrorsAsyncDelegate = (s, token, meta) => new ValueTask<ValidationResult>(ValidationResult.Get(s, s))
            };
            var validator = new Validator();
            validator.AddComponent(component);

            component.TryValidateAsync(null!, expectedMember, CancellationToken.None, DefaultMetadata);

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

            component.ClearErrors(null!, expectedMember, DefaultMetadata);
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
                GetErrorsAsyncDelegate = (s, token, meta) => new ValueTask<ValidationResult>(ValidationResult.Get(s, s))
            };
            var validator = new Validator();
            validator.AddComponent(component);
            component.TryValidateAsync(null!, expectedMember, CancellationToken.None, DefaultMetadata);

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

            component.ClearErrors(null!, null, DefaultMetadata);
            invokeCount.ShouldEqual(count);
        }

        #endregion
    }
}