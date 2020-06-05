using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Extensions;
using MugenMvvm.Internal;
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
            var result = ValidationResult.FromErrors(new Dictionary<string, ItemOrList<object, IReadOnlyList<object>>>
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

            var task = component.TryValidateAsync(expectedMember, CancellationToken.None, DefaultMetadata)!;
            if (isAsync)
            {
                task.IsCompleted.ShouldBeFalse();
                tcs.SetResult(result);
                task.IsCompleted.ShouldBeTrue();
            }

            component.TryGetErrors(expectedMember).AsList().Single().ShouldEqual(expectedMember);
            var errors = component.TryGetErrors();
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
            var task = component.TryValidateAsync(expectedMember, cts.Token)!;
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

            var task = component.TryValidateAsync(expectedMember, CancellationToken.None)!;
            task.IsCompleted.ShouldBeFalse();

            component.TryValidateAsync(expectedMember, CancellationToken.None);
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

            var task = component.TryValidateAsync(expectedMember, CancellationToken.None)!;
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
                component.TryValidateAsync(expectedMember).ShouldBeNull();
                return new ValueTask<ValidationResult>(tcs.Task);
            };
            var validator = new Validator();
            validator.AddComponent(component);

            var task = component.TryValidateAsync(expectedMember, CancellationToken.None);
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

            component.HasErrors().ShouldBeFalse();
            component.TryValidateAsync(expectedMember, CancellationToken.None);
            component.HasErrors().ShouldBeTrue();
            component.HasErrors(expectedMember).ShouldBeTrue();
            tcs.SetResult(default);
            component.HasErrors().ShouldBeFalse();
            component.HasErrors(expectedMember).ShouldBeFalse();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void GetErrorsShouldReturnErrors(int count)
        {
            var component = new TestValidatorComponentBase<object>(this, false)
            {
                GetErrorsAsyncDelegate = (s, token, _) => new ValueTask<ValidationResult>(ValidationResult.FromMemberErrors(s, s))
            };
            component.HasErrors().ShouldBeFalse();

            var validator = new Validator();
            validator.AddComponent(component);

            var members = new List<string>();
            for (var i = 0; i < count; i++)
            {
                var member = i.ToString();
                members.Add(member);
                component.TryValidateAsync(member);
            }

            component.HasErrors().ShouldBeTrue();
            foreach (var member in members)
            {
                component.TryGetErrors(member).AsList().Single().ShouldEqual(member);
                component.HasErrors(member).ShouldBeTrue();
            }

            component.TryGetErrors(string.Empty).AsList().SequenceEqual(members).ShouldBeTrue();
            var errors = component.TryGetErrors();
            errors.Count.ShouldEqual(count);
            foreach (var member in members)
                errors[member].AsList().Single().ShouldEqual(member);

            for (var i = 0; i < count; i++)
            {
                component.ClearErrors(members[0]);
                members.RemoveAt(0);
                component.TryGetErrors(string.Empty).AsList().SequenceEqual(members).ShouldBeTrue();
            }

            component.HasErrors().ShouldBeFalse();

            for (var i = 0; i < count; i++)
            {
                var member = i.ToString();
                members.Add(member);
                component.TryValidateAsync(member);
            }

            component.HasErrors().ShouldBeTrue();
            component.ClearErrors();
            component.HasErrors().ShouldBeFalse();
            component.TryGetErrors(string.Empty).AsList().ShouldBeEmpty();
            component.TryGetErrors().ShouldBeEmpty();
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
                    return new ValueTask<ValidationResult>(ValidationResult.FromMemberErrors(s, s));
                }
            };
            component.HasErrors().ShouldBeFalse();
            var validator = new Validator();
            validator.AddComponent(component);

            for (var i = 0; i < count; i++)
            {
                var member = i.ToString();
                component.TryValidateAsync(member);
                component.HasErrors(member).ShouldBeTrue();
            }

            component.HasErrors().ShouldBeTrue();
            component.TryValidateAsync(string.Empty);
            component.HasErrors().ShouldBeFalse();
            component.TryGetErrors(string.Empty).AsList().ShouldBeEmpty();
            component.TryGetErrors().ShouldBeEmpty();
        }

        [Fact]
        public void ValidateAsyncShouldUpdateAllErrorsEmptyString2()
        {
            var errors = new Dictionary<string, ItemOrList<object, IReadOnlyList<object>>>
            {
                {"test0", new[] {"1", "2"}}
            };
            var emptyStringErrors = new Dictionary<string, ItemOrList<object, IReadOnlyList<object>>>
            {
                {"test1", new[] {"1", "2"}}
            };
            var component = new TestValidatorComponentBase<object>(this, false)
            {
                GetErrorsAsyncDelegate = (s, token, _) =>
                {
                    if (string.IsNullOrEmpty(s))
                        return new ValueTask<ValidationResult>(ValidationResult.FromErrors(emptyStringErrors));
                    return new ValueTask<ValidationResult>(ValidationResult.FromErrors(errors));
                }
            };
            component.HasErrors().ShouldBeFalse();
            var validator = new Validator();
            validator.AddComponent(component);

            component.TryValidateAsync("test");
            component.TryGetErrors(errors.First().Key).AsList().SequenceEqual(errors.First().Value.AsList()).ShouldBeTrue();
            var pair = component.TryGetErrors().Single();
            pair.Key.ShouldEqual(errors.First().Key);
            pair.Value.AsList().SequenceEqual(errors.First().Value.AsList()).ShouldBeTrue();
            component.HasErrors().ShouldBeTrue();
            component.HasErrors(errors.First().Key).ShouldBeTrue();

            component.TryValidateAsync(string.Empty);
            component.TryGetErrors(emptyStringErrors.First().Key).AsList().SequenceEqual(emptyStringErrors.First().Value.AsList()).ShouldBeTrue();
            pair = component.TryGetErrors().Single();
            pair.Key.ShouldEqual(emptyStringErrors.First().Key);
            pair.Value.AsList().SequenceEqual(emptyStringErrors.First().Value.AsList()).ShouldBeTrue();
            component.HasErrors().ShouldBeTrue();
            component.HasErrors(emptyStringErrors.First().Key).ShouldBeTrue();
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
                GetErrorsAsyncDelegate = (s, token, meta) => new ValueTask<ValidationResult>(ValidationResult.FromMemberErrors(s, s))
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

            component.TryValidateAsync(expectedMember, CancellationToken.None, DefaultMetadata);
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

            var validateAsync = component.TryValidateAsync(expectedMember, CancellationToken.None, DefaultMetadata);
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
                GetErrorsAsyncDelegate = (s, token, meta) => new ValueTask<ValidationResult>(ValidationResult.FromMemberErrors(s, s))
            };
            var validator = new Validator();
            validator.AddComponent(component);

            component.TryValidateAsync(expectedMember, CancellationToken.None, DefaultMetadata);

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
                GetErrorsAsyncDelegate = (s, token, meta) => new ValueTask<ValidationResult>(ValidationResult.FromMemberErrors(s, s))
            };
            var validator = new Validator();
            validator.AddComponent(component);
            component.TryValidateAsync(expectedMember, CancellationToken.None, DefaultMetadata);

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