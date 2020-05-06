using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Validation.Components;
using MugenMvvm.Metadata;
using MugenMvvm.UnitTest.Components;
using MugenMvvm.UnitTest.Validation.Internal;
using MugenMvvm.Validation;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Validation
{
    public class AggregatorValidatorTest : ComponentOwnerTestBase<AggregatorValidator>
    {
        #region Methods

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void DisposeShouldClearComponentsMetadataNotifyListeners(int count)
        {
            var invokeCount = 0;
            var validator = new AggregatorValidator();

            for (var i = 0; i < count; i++)
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

            validator.IsDisposed.ShouldBeFalse();
            validator.Metadata.Set(ValidationMetadata.IgnoreMembers, new List<string>());
            validator.Dispose();
            validator.IsDisposed.ShouldBeTrue();
            invokeCount.ShouldEqual(count);
            validator.Components.Count.ShouldEqual(0);
            validator.Metadata.Count.ShouldEqual(0);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void HasErrorsShouldBeHandledByComponents(int componentCount)
        {
            var count = 0;
            var hasErrors = false;
            var validator = GetComponentOwner();
            validator.HasErrors.ShouldBeFalse();
            for (var i = 0; i < componentCount; i++)
            {
                var component = new TestValidator
                {
                    HasErrors = () =>
                    {
                        ++count;
                        return hasErrors;
                    }
                };
                validator.AddComponent(component);
            }

            validator.HasErrors.ShouldBeFalse();
            count.ShouldEqual(componentCount);

            count = 0;
            hasErrors = true;
            validator.HasErrors.ShouldBeTrue();
            count.ShouldEqual(1);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void GetErrorsShouldBeHandledByComponents(int componentCount)
        {
            var memberName = "test";
            var count = 0;
            var validator = GetComponentOwner();
            validator.GetErrors(memberName, DefaultMetadata).ShouldBeEmpty();

            for (var i = 0; i < componentCount; i++)
            {
                var s = i.ToString();
                var component = new TestValidator
                {
                    GetErrors = (m, metadata) =>
                    {
                        ++count;
                        m.ShouldEqual(memberName);
                        metadata.ShouldEqual(DefaultMetadata);
                        return new[] { s };
                    }
                };
                validator.AddComponent(component);
            }

            var errors = validator.GetErrors(memberName, DefaultMetadata);
            errors.Count.ShouldEqual(componentCount);
            for (var i = 0; i < componentCount; i++)
                errors.ShouldContain(i.ToString());
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void GetAllErrorsShouldBeHandledByComponents(int componentCount)
        {
            var count = 0;
            var validator = GetComponentOwner();
            validator.GetErrors(DefaultMetadata).ShouldBeEmpty();

            for (var i = 0; i < componentCount; i++)
            {
                var s = i.ToString();
                var component = new TestValidator
                {
                    GetAllErrors = metadata =>
                    {
                        ++count;
                        metadata.ShouldEqual(DefaultMetadata);
                        return new Dictionary<string, IReadOnlyList<object>>
                        {
                            {s, new[] {s}}
                        };
                    }
                };
                validator.AddComponent(component);
            }

            var errors = validator.GetErrors(DefaultMetadata);
            errors.Count.ShouldEqual(componentCount);
            for (var i = 0; i < componentCount; i++)
                errors[i.ToString()].Single().ShouldEqual(i.ToString());
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void ValidateAsyncShouldBeHandledByComponents(int componentCount)
        {
            var memberName = "test";
            var count = 0;
            var validator = GetComponentOwner();
            var cts = new CancellationTokenSource();
            var tasks = new List<TaskCompletionSource<object>>();

            for (var i = 0; i < componentCount; i++)
            {
                var tcs = new TaskCompletionSource<object>();
                tasks.Add(tcs);
                var component = new TestValidator
                {
                    ValidateAsync = (m, token, metadata) =>
                    {
                        ++count;
                        m.ShouldEqual(memberName);
                        token.ShouldEqual(cts.Token);
                        metadata.ShouldEqual(DefaultMetadata);
                        return tcs.Task;
                    }
                };
                validator.AddComponent(component);
            }

            var task = validator.ValidateAsync(memberName, cts.Token, DefaultMetadata);
            task.IsCompleted.ShouldBeFalse();

            for (var i = 0; i < componentCount - 1; i++)
                tasks[i].SetResult(i);
            task.IsCompleted.ShouldBeFalse();
            tasks.Last().SetResult("");
            task.IsCompleted.ShouldBeTrue();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void ClearErrorsShouldBeHandledByComponents(int componentCount)
        {
            var memberName = "test";
            var count = 0;
            var validator = GetComponentOwner();

            for (var i = 0; i < componentCount; i++)
            {
                var s = i.ToString();
                var component = new TestValidator
                {
                    ClearErrors = (m, metadata) =>
                    {
                        ++count;
                        m.ShouldEqual(memberName);
                        metadata.ShouldEqual(DefaultMetadata);
                    }
                };
                validator.AddComponent(component);
            }

            validator.ClearErrors(memberName, DefaultMetadata);
            count.ShouldEqual(componentCount);
        }

        [Fact]
        public void AddValidatorShouldIgnoreSelf()
        {
            var validator = GetComponentOwner();
            validator.AddValidator(validator).ShouldBeFalse();
            validator.Components.Count.ShouldEqual(0);
        }

        [Fact]
        public void AddValidatorShouldNotAddDuplicates()
        {
            var validator = GetComponentOwner();
            var testValidator = new TestValidator();
            validator.AddValidator(testValidator).ShouldBeTrue();
            validator.AddValidator(testValidator).ShouldBeFalse();
            validator.Components.Count.ShouldEqual(1);
        }

        [Fact]
        public void AddRemoveValidatorShouldAddListener()
        {
            var validator = GetComponentOwner();
            var testValidator = new TestValidator();
            validator.AddValidator(testValidator);

            testValidator.Components.Count.ShouldEqual(1);
            testValidator.GetComponent<IValidatorListener>().ShouldEqual(validator);

            validator.RemoveValidator(testValidator).ShouldBeTrue();
            testValidator.Components.Count.ShouldEqual(0);
        }

        [Fact]
        public void DisposeChildValidatorShouldRemoveIt()
        {
            var validator = GetComponentOwner();
            var testValidator = new TestValidator();
            validator.AddValidator(testValidator);

            validator.Components.Count.ShouldEqual(1);
            testValidator.GetComponent<IValidatorListener>().OnDisposed(testValidator);
            validator.Components.Count.ShouldEqual(0);
        }

        [Fact]
        public void ShouldResendChildEvents()
        {
            string member = "test";
            int asyncCount = 0;
            var errorsCount = 0;
            var validator = GetComponentOwner();
            var testValidator = new TestValidator();
            validator.AddValidator(testValidator);
            var testListener = new TestValidatorListener
            {
                OnAsyncValidation = (v, m, t, context) =>
                {
                    ++asyncCount;
                    v.ShouldEqual(validator);
                    m.ShouldEqual(member);
                    t.ShouldEqual(Task.CompletedTask);
                    context.ShouldEqual(DefaultMetadata);
                },
                OnErrorsChanged = (v, m, context) =>
                {
                    ++errorsCount;
                    v.ShouldEqual(validator);
                    m.ShouldEqual(member);
                    context.ShouldEqual(DefaultMetadata);
                }
            };
            validator.AddComponent(testListener);

            var listener = testValidator.GetComponent<IValidatorListener>();
            listener.OnErrorsChanged(testValidator, member, DefaultMetadata);
            asyncCount.ShouldEqual(0);
            errorsCount.ShouldEqual(1);

            listener.OnAsyncValidation(testValidator, member, Task.CompletedTask, DefaultMetadata);
            asyncCount.ShouldEqual(1);
            errorsCount.ShouldEqual(1);
        }

        protected override AggregatorValidator GetComponentOwner(IComponentCollectionProvider? collectionProvider = null)
        {
            return new AggregatorValidator(null, collectionProvider);
        }

        #endregion
    }
}