﻿using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Metadata;
using MugenMvvm.UnitTest.Components;
using MugenMvvm.UnitTest.Validation.Internal;
using MugenMvvm.Validation;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Validation
{
    public class ValidatorTest : ComponentOwnerTestBase<Validator>
    {
        #region Methods

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void DisposeShouldClearComponentsMetadataNotifyListeners(int count)
        {
            var invokeCount = 0;
            var invokeComponentCount = 0;
            var validator = new Validator();

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
                validator.AddComponent(new TestValidatorComponent
                {
                    Dispose = () => ++invokeComponentCount
                });
            }

            validator.IsDisposed.ShouldBeFalse();
            validator.Metadata.Set(MetadataContextKey.FromKey<object?, object?>("t"), "");
            validator.Dispose();
            validator.IsDisposed.ShouldBeTrue();
            invokeCount.ShouldEqual(count);
            invokeComponentCount.ShouldEqual(count);
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
                var component = new TestValidatorComponent
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
                var component = new TestValidatorComponent
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
                var component = new TestValidatorComponent
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
                var component = new TestValidatorComponent
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
                var component = new TestValidatorComponent
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

        protected override Validator GetComponentOwner(IComponentCollectionProvider? collectionProvider = null)
        {
            return new Validator(null, collectionProvider);
        }

        #endregion
    }
}