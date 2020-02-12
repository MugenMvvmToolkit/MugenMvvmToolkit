using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Metadata;
using MugenMvvm.UnitTest.Components;
using MugenMvvm.UnitTest.Metadata;
using MugenMvvm.Validation;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Validation
{
    public class ValidatorBaseTest : ComponentOwnerTestBase<ValidatorBase<object>>
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
            var validator = new TestValidator<object>(isAsync);
            validator.Initialize(this);
            validator.GetErrorsAsyncDelegate = (s, token, meta) =>
            {
                ++invokeCount;
                s.ShouldEqual(expectedMember);
                meta.ShouldEqual(DefaultMetadata);
                return new ValueTask<ValidationResult>(tcs.Task);
            };
            if (!isAsync)
                tcs.SetResult(result);

            var task = validator.ValidateAsync(expectedMember, CancellationToken.None, DefaultMetadata);
            if (isAsync)
            {
                task.IsCompleted.ShouldBeFalse();
                tcs.SetResult(result);
                task.IsCompleted.ShouldBeTrue();
            }

            validator.GetErrors(expectedMember).Single().ShouldEqual(expectedMember);
            var errors = validator.GetErrors();
            errors.Count.ShouldEqual(1);
            errors[expectedMember].Single().ShouldEqual(expectedMember);
        }

        [Fact]
        public void ValidateAsyncShouldUseCancellationToken()
        {
            var expectedMember = "test";
            var validator = new TestValidator<object>(true);
            validator.Initialize(this);
            var tcs = new TaskCompletionSource<ValidationResult>();
            validator.GetErrorsAsyncDelegate = (s, token, _) =>
            {
                token.Register(() => tcs.SetCanceled());
                return new ValueTask<ValidationResult>(tcs.Task);
            };

            var cts = new CancellationTokenSource();
            var task = validator.ValidateAsync(expectedMember, cts.Token);
            task.IsCompleted.ShouldBeFalse();

            cts.Cancel();
            task.IsCanceled.ShouldBeTrue();
        }

        [Fact]
        public void ValidateAsyncShouldCancelPreviousValidation()
        {
            var expectedMember = "test";
            var validator = new TestValidator<object>(true);
            validator.Initialize(this);
            var tcs = new TaskCompletionSource<ValidationResult>();
            validator.GetErrorsAsyncDelegate = (s, token, _) =>
            {
                token.Register(() => tcs.SetCanceled());
                return new ValueTask<ValidationResult>(tcs.Task);
            };

            var task = validator.ValidateAsync(expectedMember, CancellationToken.None);
            task.IsCompleted.ShouldBeFalse();

            validator.ValidateAsync(expectedMember, CancellationToken.None);
            task.IsCanceled.ShouldBeTrue();
        }

        [Fact]
        public void ValidateAsyncShouldBeCanceledDispose()
        {
            var expectedMember = "test";
            var validator = new TestValidator<object>(true);
            validator.Initialize(this);
            var tcs = new TaskCompletionSource<ValidationResult>();
            validator.GetErrorsAsyncDelegate = (s, token, _) =>
            {
                token.Register(() => tcs.SetCanceled());
                return new ValueTask<ValidationResult>(tcs.Task);
            };

            var task = validator.ValidateAsync(expectedMember, CancellationToken.None);
            task.IsCompleted.ShouldBeFalse();

            validator.Dispose();
            task.IsCanceled.ShouldBeTrue();
        }

        [Fact]
        public void ValidateAsyncShouldHandleCycles()
        {
            var expectedMember = "test";
            int invokeCount = 0;
            var validator = new TestValidator<object>(true);
            validator.Initialize(this);
            var tcs = new TaskCompletionSource<ValidationResult>();
            validator.GetErrorsAsyncDelegate = (s, token, _) =>
            {
                ++invokeCount;
                validator.ValidateAsync(expectedMember).IsCompleted.ShouldBeTrue();
                return new ValueTask<ValidationResult>(tcs.Task);
            };

            var task = validator.ValidateAsync(expectedMember, CancellationToken.None);
            task.IsCompleted.ShouldBeFalse();
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public void ValidateAsyncShouldIgnoreMembers()
        {
            int invokeCount = 0;
            var expectedMember = "test";
            var validator = new TestValidator<object>(true);
            validator.Initialize(this);
            validator.GetErrorsAsyncDelegate = (s, token, _) =>
            {
                ++invokeCount;
                return default;
            };
            validator.Metadata.Set(ValidationMetadata.IgnoreMembers, new List<string> { expectedMember });

            var task = validator.ValidateAsync(expectedMember, CancellationToken.None);
            task.IsCompleted.ShouldBeTrue();
            invokeCount.ShouldEqual(0);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void DisposeShouldClearComponentsMetadataNotifyListeners(int count)
        {
            int invokeCount = 0;
            var validator = new TestValidator<object>(true);
            validator.Initialize(this);

            for (int i = 0; i < count; i++)
            {
                validator.AddComponent(new TestValidatorListener
                {
                    OnDisposed = v =>
                    {
                        ++invokeCount;
                        v.ShouldEqual(validator);
                    }
                });
            }

            validator.Metadata.Set(ValidationMetadata.IgnoreMembers, new List<string>());
            validator.Dispose();
            invokeCount.ShouldEqual(count);
            validator.Components.Count.ShouldEqual(0);
            validator.Metadata.Count.ShouldEqual(0);
        }

        [Fact]
        public void HasErrorsShouldReturnTrueAsync()
        {
            var expectedMember = "test";
            var validator = new TestValidator<object>(true);
            validator.Initialize(this);
            var tcs = new TaskCompletionSource<ValidationResult>();
            validator.GetErrorsAsyncDelegate = (s, token, _) => new ValueTask<ValidationResult>(tcs.Task);

            validator.HasErrors.ShouldBeFalse();
            validator.ValidateAsync(expectedMember, CancellationToken.None);
            validator.HasErrors.ShouldBeTrue();
            tcs.SetResult(default);
            validator.HasErrors.ShouldBeFalse();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void GetErrorsShouldReturnErrors(int count)
        {
            var validator = new TestValidator<object>(false) { GetErrorsAsyncDelegate = (s, token, _) => new ValueTask<ValidationResult>(ValidationResult.SingleResult(s, s)) };
            validator.Initialize(this);
            validator.HasErrors.ShouldBeFalse();

            var members = new List<string>();
            for (int i = 0; i < count; i++)
            {
                var member = i.ToString();
                members.Add(member);
                validator.ValidateAsync(member);
            }

            validator.HasErrors.ShouldBeTrue();
            foreach (var member in members)
                validator.GetErrors(member).Single().ShouldEqual(member);

            validator.GetErrors(string.Empty).SequenceEqual(members).ShouldBeTrue();
            var errors = validator.GetErrors();
            errors.Count.ShouldEqual(count);
            foreach (var member in members)
                errors[member].Single().ShouldEqual(member);

            for (int i = 0; i < count; i++)
            {
                validator.ClearErrors(members[0]);
                members.RemoveAt(0);
                validator.GetErrors(string.Empty).SequenceEqual(members).ShouldBeTrue();
            }

            validator.HasErrors.ShouldBeFalse();

            for (int i = 0; i < count; i++)
            {
                var member = i.ToString();
                members.Add(member);
                validator.ValidateAsync(member);
            }

            validator.HasErrors.ShouldBeTrue();
            validator.ClearErrors();
            validator.HasErrors.ShouldBeFalse();
            validator.GetErrors(string.Empty).ShouldBeEmpty();
            validator.GetErrors().ShouldBeEmpty();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void ValidateAsyncShouldUpdateAllErrorsEmptyString(int count)
        {
            var validator = new TestValidator<object>(false)
            {
                GetErrorsAsyncDelegate = (s, token, _) =>
                {

                    if (string.IsNullOrEmpty(s))
                        return new ValueTask<ValidationResult>(ValidationResult.NoErrors);
                    return new ValueTask<ValidationResult>(ValidationResult.SingleResult(s, s));
                }
            };
            validator.Initialize(this);
            validator.HasErrors.ShouldBeFalse();

            var members = new List<string>();
            for (int i = 0; i < count; i++)
            {
                var member = i.ToString();
                members.Add(member);
                validator.ValidateAsync(member);
            }

            validator.ValidateAsync(string.Empty);
            validator.HasErrors.ShouldBeFalse();
            validator.GetErrors(string.Empty).ShouldBeEmpty();
            validator.GetErrors().ShouldBeEmpty();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void ValidateAsyncShouldNotifyListeners1(int count)
        {
            var invokeCount = 0;
            var expectedMember = "test";
            var validator = new TestValidator<object>(false);
            validator.Initialize(this);
            validator.GetErrorsAsyncDelegate = (s, token, meta) => new ValueTask<ValidationResult>(ValidationResult.SingleResult(s, s));

            for (int i = 0; i < count; i++)
            {
                var listener = new TestValidatorListener
                {
                    OnErrorsChanged = (v, s, arg3) =>
                    {
                        ++invokeCount;
                        v.ShouldEqual(validator);
                        s.ShouldEqual(expectedMember);
                        arg3.ShouldEqual(DefaultMetadata);
                    }
                };
                validator.AddComponent(listener);
            }

            validator.ValidateAsync(expectedMember, CancellationToken.None, DefaultMetadata);
            invokeCount.ShouldEqual(count);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void ValidateAsyncShouldNotifyListeners2(int count)
        {
            var invokeCount = 0;
            var expectedMember = "test";
            var validator = new TestValidator<object>(true);
            Task? expectedTask = null;
            validator.Initialize(this);
            validator.GetErrorsAsyncDelegate = (s, token, meta) => new ValueTask<ValidationResult>(new TaskCompletionSource<ValidationResult>().Task);

            for (int i = 0; i < count; i++)
            {
                var listener = new TestValidatorListener
                {
                    OnAsyncValidation = (v, s, task, context) =>
                    {
                        ++invokeCount;
                        expectedTask = task;
                        v.ShouldEqual(validator);
                        s.ShouldEqual(expectedMember);
                        context.ShouldEqual(DefaultMetadata);
                    }
                };
                validator.AddComponent(listener);
            }

            var validateAsync = validator.ValidateAsync(expectedMember, CancellationToken.None, DefaultMetadata);
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
            var validator = new TestValidator<object>(false);
            validator.Initialize(this);
            validator.GetErrorsAsyncDelegate = (s, token, meta) => new ValueTask<ValidationResult>(ValidationResult.SingleResult(s, s));
            validator.ValidateAsync(expectedMember, CancellationToken.None, DefaultMetadata);

            for (int i = 0; i < count; i++)
            {
                var listener = new TestValidatorListener
                {
                    OnErrorsChanged = (v, s, arg3) =>
                    {
                        ++invokeCount;
                        v.ShouldEqual(validator);
                        s.ShouldEqual(expectedMember);
                        arg3.ShouldEqual(DefaultMetadata);
                    }
                };
                validator.AddComponent(listener);
            }

            validator.ClearErrors(expectedMember, DefaultMetadata);
            invokeCount.ShouldEqual(count);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void ClearShouldNotifyListeners2(int count)
        {
            var invokeCount = 0;
            var expectedMember = "test";
            var validator = new TestValidator<object>(false);
            validator.Initialize(this);
            validator.GetErrorsAsyncDelegate = (s, token, meta) => new ValueTask<ValidationResult>(ValidationResult.SingleResult(s, s));
            validator.ValidateAsync(expectedMember, CancellationToken.None, DefaultMetadata);

            for (int i = 0; i < count; i++)
            {
                var listener = new TestValidatorListener
                {
                    OnErrorsChanged = (v, s, arg3) =>
                    {
                        ++invokeCount;
                        v.ShouldEqual(validator);
                        s.ShouldEqual(expectedMember);
                        arg3.ShouldEqual(DefaultMetadata);
                    }
                };
                validator.AddComponent(listener);
            }

            validator.ClearErrors(null, DefaultMetadata);
            invokeCount.ShouldEqual(count);
        }

        protected override ValidatorBase<object> GetComponentOwner(IComponentCollectionProvider? collectionProvider = null)
        {
            return new TestValidator<object>(false, componentCollectionProvider: collectionProvider);
        }

        #endregion
    }

    public class ValidatorBaseMetadataOwnerTest : MetadataOwnerTestBase
    {
        #region Methods

        protected override IMetadataOwner<IMetadataContext> GetMetadataOwner(IReadOnlyMetadataContext? metadata, IMetadataContextProvider? metadataContextProvider)
        {
            return new TestValidator<object>(false, metadata, metadataContextProvider: metadataContextProvider);
        }

        #endregion
    }
}