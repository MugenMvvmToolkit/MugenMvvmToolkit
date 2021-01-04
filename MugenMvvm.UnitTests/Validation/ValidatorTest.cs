using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Metadata;
using MugenMvvm.UnitTests.Components;
using MugenMvvm.UnitTests.Validation.Internal;
using MugenMvvm.Validation;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Validation
{
    public class ValidatorTest : ComponentOwnerTestBase<Validator>
    {
        #region Methods

        [Theory]
        [InlineData(1, true)]
        [InlineData(10, true)]
        [InlineData(1, false)]
        [InlineData(10, false)]
        public void DisposeShouldClearComponentsMetadataNotifyListeners(int count, bool canDispose)
        {
            var invokeCount = 0;
            var invokeComponentCount = 0;
            var validator = new Validator();
            validator.IsDisposable.ShouldBeTrue();
            validator.IsDisposable = canDispose;

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
            validator.Metadata.Set(MetadataContextKey.FromKey<object?>("t"), "");
            validator.Dispose();
            if (canDispose)
            {
                validator.IsDisposed.ShouldBeTrue();
                invokeCount.ShouldEqual(count);
                invokeComponentCount.ShouldEqual(count);
                validator.Components.Count.ShouldEqual(0);
                validator.Metadata.Count.ShouldEqual(0);
            }
            else
            {
                validator.IsDisposed.ShouldBeFalse();
                invokeCount.ShouldEqual(0);
                invokeComponentCount.ShouldEqual(0);
                validator.Components.Count.ShouldEqual(count * 2);
            }
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void HasErrorsShouldBeHandledByComponents(int componentCount)
        {
            string? expectedMember = null;
            var count = 0;
            var hasErrors = false;
            var validator = GetComponentOwner();
            validator.HasErrors().ShouldBeFalse();
            for (var i = 0; i < componentCount; i++)
            {
                var component = new TestValidatorComponent
                {
                    HasErrors = (v, s, m) =>
                    {
                        ++count;
                        v.ShouldEqual(validator);
                        s.ShouldEqual(expectedMember);
                        m.ShouldEqual(DefaultMetadata);
                        return hasErrors;
                    },
                    Priority = -i
                };
                validator.AddComponent(component);
            }

            validator.HasErrors(expectedMember, DefaultMetadata).ShouldBeFalse();
            count.ShouldEqual(componentCount);

            count = 0;
            expectedMember = "t";
            validator.HasErrors(expectedMember, DefaultMetadata).ShouldBeFalse();
            count.ShouldEqual(componentCount);

            count = 0;
            hasErrors = true;
            validator.HasErrors(expectedMember, DefaultMetadata).ShouldBeTrue();
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
            validator.GetErrors(memberName, DefaultMetadata).AsList().ShouldBeEmpty();

            for (var i = 0; i < componentCount; i++)
            {
                var s = i.ToString();
                var component = new TestValidatorComponent
                {
                    GetErrors = (v, m, metadata) =>
                    {
                        ++count;
                        v.ShouldEqual(validator);
                        m.ShouldEqual(memberName);
                        metadata.ShouldEqual(DefaultMetadata);
                        return new[] {s};
                    }
                };
                validator.AddComponent(component);
            }

            var errors = validator.GetErrors(memberName, DefaultMetadata).AsList();
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
                    GetAllErrors = (v, metadata) =>
                    {
                        ++count;
                        v.ShouldEqual(validator);
                        metadata.ShouldEqual(DefaultMetadata);
                        return new Dictionary<string, object>
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
                errors[i.ToString()].AsItemOrList().AsList().Single().ShouldEqual(i.ToString());
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
                    ValidateAsync = (v, m, token, metadata) =>
                    {
                        ++count;
                        v.ShouldEqual(validator);
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
                    ClearErrors = (v, m, metadata) =>
                    {
                        ++count;
                        v.ShouldEqual(validator);
                        m.ShouldEqual(memberName);
                        metadata.ShouldEqual(DefaultMetadata);
                    }
                };
                validator.AddComponent(component);
            }

            validator.ClearErrors(memberName, DefaultMetadata);
            count.ShouldEqual(componentCount);
        }

        protected override Validator GetComponentOwner(IComponentCollectionManager? collectionProvider = null) => new(null, collectionProvider);

        #endregion
    }
}