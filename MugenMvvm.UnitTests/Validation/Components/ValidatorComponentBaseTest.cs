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
        [Fact]
        public async Task ValidateAsyncShouldUpdateAllErrorsEmptyString2()
        {
            var errors = new Dictionary<string, object?>
            {
                {"test0", new[] {"1", "2"}}
            };
            var emptyStringErrors = new Dictionary<string, object?>
            {
                {"test1", new[] {"1", "2"}}
            };
            var component = new TestValidatorComponentBase<object>(this)
            {
                GetErrorsAsyncDelegate = (s, token, _) =>
                {
                    if (string.IsNullOrEmpty(s))
                        return new ValueTask<ValidationResult>(ValidationResult.Get(emptyStringErrors));
                    return new ValueTask<ValidationResult>(ValidationResult.Get(errors));
                }
            };
            var validator = new Validator();
            validator.AddComponent(component);

            component.HasErrors(validator, null, null).ShouldBeFalse();
            await component.TryValidateAsync(validator, "test");
            component.TryGetErrors(validator, errors.First().Key).AsList().ShouldEqual((IEnumerable<object>) errors.First().Value!);
            var pair = component.TryGetErrors(validator).Single();
            pair.Key.ShouldEqual(errors.First().Key);
            pair.Value.AsItemOrList().AsList().ShouldEqual((IEnumerable<object>) errors.First().Value!);
            component.HasErrors(validator, null, null).ShouldBeTrue();
            component.HasErrors(validator, errors.First().Key, null).ShouldBeTrue();

            await component.TryValidateAsync(validator, string.Empty);
            component.TryGetErrors(validator, emptyStringErrors.First().Key).AsList().ShouldEqual((IEnumerable<object>) emptyStringErrors.First().Value!);
            pair = component.TryGetErrors(validator).Single();
            pair.Key.ShouldEqual(emptyStringErrors.First().Key);
            pair.Value.AsItemOrList().AsList().ShouldEqual((IEnumerable<object>) emptyStringErrors.First().Value!);
            component.HasErrors(validator, null, null).ShouldBeTrue();
            component.HasErrors(validator, emptyStringErrors.First().Key, null).ShouldBeTrue();
        }

        [Fact]
        public async Task ValidateAsyncShouldUseCancellationToken()
        {
            var expectedMember = "test";
            var tcs = new TaskCompletionSource<ValidationResult>();
            var component = new TestValidatorComponentBase<object>(this)
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
            var task = component.TryValidateAsync(validator, expectedMember, cts.Token)!;
            task.IsCompleted.ShouldBeFalse();

            cts.Cancel();
            await task.WaitSafeAsync();
            task.IsCanceled.ShouldBeTrue();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ValidateAsyncShouldUpdateErrors(bool isAsync)
        {
            var invokeCount = 0;
            var expectedMember = "test";
            var tcs = new TaskCompletionSource<ValidationResult>();
            var result = ValidationResult.Get(new Dictionary<string, object?>
            {
                {expectedMember, new[] {expectedMember}}
            });
            var component = new TestValidatorComponentBase<object>(this)
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

            var task = component.TryValidateAsync(validator, expectedMember, CancellationToken.None, DefaultMetadata)!;
            if (isAsync)
            {
                task.IsCompleted.ShouldBeFalse();
                tcs.SetResult(result);
                await task;
                task.IsCompleted.ShouldBeTrue();
            }

            component.TryGetErrors(validator, expectedMember).AsList().Single().ShouldEqual(expectedMember);
            var errors = component.TryGetErrors(validator);
            errors.Count.ShouldEqual(1);
            errors[expectedMember].AsItemOrList().AsList().Single().ShouldEqual(expectedMember);
            invokeCount.ShouldEqual(1);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ValidateAsyncShouldBeCanceledDispose(bool canDispose)
        {
            var expectedMember = "test";
            var tcs = new TaskCompletionSource<ValidationResult>();
            var component = new TestValidatorComponentBase<object>(this)
            {
                GetErrorsAsyncDelegate = (s, token, _) =>
                {
                    token.Register(() => tcs.SetCanceled());
                    return new ValueTask<ValidationResult>(tcs.Task);
                }
            };
            component.IsDisposable.ShouldBeTrue();
            component.IsDisposable = canDispose;
            var validator = new Validator();
            validator.AddComponent(component);

            var task = component.TryValidateAsync(validator, expectedMember, CancellationToken.None)!;
            task.IsCompleted.ShouldBeFalse();

            component.Dispose();
            if (canDispose)
            {
                await task.WaitSafeAsync();
                task.IsCanceled.ShouldBeTrue();
            }
            else
            {
                WaitCompletion();
                tcs.Task.IsCompleted.ShouldBeFalse();
            }
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public async Task GetErrorsShouldReturnErrors(int count)
        {
            var component = new TestValidatorComponentBase<object>(this)
            {
                GetErrorsAsyncDelegate = (s, token, _) => new ValueTask<ValidationResult>(ValidationResult.Get(s, s))
            };

            var validator = new Validator();
            validator.AddComponent(component);
            component.HasErrors(validator, null, null).ShouldBeFalse();

            var members = new List<string>();
            for (var i = 0; i < count; i++)
            {
                var member = i.ToString();
                members.Add(member);
                component.TryValidateAsync(validator, member);
            }

            component.HasErrors(validator, null, null).ShouldBeTrue();
            foreach (var member in members)
            {
                component.TryGetErrors(validator, member).AsList().Single().ShouldEqual(member);
                component.HasErrors(validator, member, null).ShouldBeTrue();
            }

            component.TryGetErrors(validator, string.Empty).AsList().ShouldEqual(members);
            var errors = component.TryGetErrors(validator);
            errors.Count.ShouldEqual(count);
            foreach (var member in members)
                errors[member].AsItemOrList().AsList().Single().ShouldEqual(member);

            for (var i = 0; i < count; i++)
            {
                component.ClearErrors(validator, members[0]);
                members.RemoveAt(0);
                component.TryGetErrors(validator, string.Empty).AsList().ShouldEqual(members);
            }

            component.HasErrors(validator, null, null).ShouldBeFalse();

            for (var i = 0; i < count; i++)
            {
                var member = i.ToString();
                members.Add(member);
                await component.TryValidateAsync(validator, member);
            }

            component.HasErrors(validator, null, null).ShouldBeTrue();
            component.ClearErrors(validator);
            component.HasErrors(validator, null, null).ShouldBeFalse();
            component.TryGetErrors(validator, string.Empty).AsList().ShouldBeEmpty();
            component.TryGetErrors(validator).ShouldBeEmpty();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public async Task ValidateAsyncShouldUpdateAllErrorsEmptyString1(int count)
        {
            var component = new TestValidatorComponentBase<object>(this)
            {
                GetErrorsAsyncDelegate = (s, token, _) =>
                {
                    if (string.IsNullOrEmpty(s))
                        return new ValueTask<ValidationResult>(ValidationResult.NoErrors);
                    return new ValueTask<ValidationResult>(ValidationResult.Get(s, s));
                }
            };
            var validator = new Validator();
            validator.AddComponent(component);
            component.HasErrors(validator, null, null).ShouldBeFalse();

            for (var i = 0; i < count; i++)
            {
                var member = i.ToString();
                component.TryValidateAsync(validator, member);
                component.HasErrors(validator, member, null).ShouldBeTrue();
            }

            component.HasErrors(validator, null, null).ShouldBeTrue();
            await component.TryValidateAsync(validator, string.Empty);
            component.HasErrors(validator, null, null).ShouldBeFalse();
            component.TryGetErrors(validator, string.Empty).AsList().ShouldBeEmpty();
            component.TryGetErrors(validator).ShouldBeEmpty();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public async Task ValidateAsyncShouldNotifyListeners1(int count)
        {
            var invokeCount = 0;
            var expectedMember = "test";
            var component = new TestValidatorComponentBase<object>(this)
            {
                GetErrorsAsyncDelegate = (s, token, meta) => new ValueTask<ValidationResult>(ValidationResult.Get(s, s, DefaultMetadata))
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

            await component.TryValidateAsync(validator, expectedMember, CancellationToken.None, DefaultMetadata);
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
            var component = new TestValidatorComponentBase<object>(this)
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

            var validateAsync = component.TryValidateAsync(validator, expectedMember, CancellationToken.None, DefaultMetadata);
            invokeCount.ShouldEqual(count);
            validateAsync.ShouldEqual(expectedTask);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public async Task ClearShouldNotifyListeners1(int count)
        {
            var invokeCount = 0;
            var expectedMember = "test";
            var component = new TestValidatorComponentBase<object>(this)
            {
                GetErrorsAsyncDelegate = (s, token, meta) => new ValueTask<ValidationResult>(ValidationResult.Get(s, s))
            };
            var validator = new Validator();
            validator.AddComponent(component);

            await component.TryValidateAsync(validator, expectedMember, CancellationToken.None, DefaultMetadata);

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

            component.ClearErrors(validator, expectedMember, DefaultMetadata);
            invokeCount.ShouldEqual(count);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public async Task ClearShouldNotifyListeners2(int count)
        {
            var invokeCount = 0;
            var expectedMember = "test";
            var component = new TestValidatorComponentBase<object>(this)
            {
                GetErrorsAsyncDelegate = (s, token, meta) => new ValueTask<ValidationResult>(ValidationResult.Get(s, s))
            };
            var validator = new Validator();
            validator.AddComponent(component);
            await component.TryValidateAsync(validator, expectedMember, CancellationToken.None, DefaultMetadata);

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

            component.ClearErrors(validator, null, DefaultMetadata);
            invokeCount.ShouldEqual(count);
        }
    }
}