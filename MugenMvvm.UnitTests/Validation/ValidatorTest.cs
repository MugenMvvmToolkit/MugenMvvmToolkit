using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Collections;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Metadata;
using MugenMvvm.UnitTests.Components;
using MugenMvvm.UnitTests.Models.Internal;
using MugenMvvm.UnitTests.Validation.Internal;
using MugenMvvm.Validation;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Validation
{
    public class ValidatorTest : ComponentOwnerTestBase<Validator>
    {
        [Theory]
        [InlineData(1, true)]
        [InlineData(10, true)]
        [InlineData(1, false)]
        [InlineData(10, false)]
        public void DisposeShouldClearComponentsMetadataNotifyListeners(int count, bool canDispose)
        {
            var invokeCount = 0;
            var validator = GetComponentOwner(ComponentCollectionManager);
            validator.IsDisposable.ShouldBeTrue();
            validator.IsDisposable = canDispose;

            for (var i = 0; i < count; i++)
            {
                validator.Components.Add(new TestDisposable
                {
                    Dispose = () => { ++invokeCount; }
                });
            }

            validator.IsDisposed.ShouldBeFalse();
            validator.Metadata.Set(MetadataContextKey.FromKey<object?>("t"), "");
            validator.Dispose();
            if (canDispose)
            {
                validator.IsDisposed.ShouldBeTrue();
                invokeCount.ShouldEqual(count);
                validator.Components.Count.ShouldEqual(0);
                validator.Metadata.Count.ShouldEqual(0);
            }
            else
            {
                validator.IsDisposed.ShouldBeFalse();
                invokeCount.ShouldEqual(0);
                validator.Components.Count.ShouldEqual(count);
            }
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void ValidateAsyncShouldBeHandledByComponents(int componentCount)
        {
            var memberName = "test";
            var count = 0;
            var validator = GetComponentOwner(ComponentCollectionManager);
            var cts = new CancellationTokenSource();
            var tasks = new List<TaskCompletionSource<object>>();

            for (var i = 0; i < componentCount; i++)
            {
                var tcs = new TaskCompletionSource<object>();
                tasks.Add(tcs);
                var component = new TestValidationHandlerComponent(validator)
                {
                    TryValidateAsync = (m, token, metadata) =>
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
        public void HasErrorsShouldBeHandledByComponents(int componentCount)
        {
            ItemOrIReadOnlyList<string> expectedMember = new[] {"1", "2"};
            var source = new object();
            var count = 0;
            var hasErrors = false;
            var validator = GetComponentOwner(ComponentCollectionManager);
            validator.HasErrors().ShouldBeFalse();
            for (var i = 0; i < componentCount; i++)
            {
                var component = new TestValidatorErrorManagerComponent(validator)
                {
                    HasErrors = (m, s, meta) =>
                    {
                        ++count;
                        m.ShouldEqual(expectedMember);
                        s.ShouldEqual(source);
                        meta.ShouldEqual(DefaultMetadata);
                        return hasErrors;
                    },
                    Priority = -i
                };
                validator.AddComponent(component);
            }

            validator.HasErrors(expectedMember, source, DefaultMetadata).ShouldBeFalse();
            count.ShouldEqual(componentCount);

            count = 0;
            expectedMember = "t";
            validator.HasErrors(expectedMember, source, DefaultMetadata).ShouldBeFalse();
            count.ShouldEqual(componentCount);

            count = 0;
            hasErrors = true;
            validator.HasErrors(expectedMember, source, DefaultMetadata).ShouldBeTrue();
            count.ShouldEqual(1);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void GetErrorsRawShouldBeHandledByComponents(int componentCount)
        {
            var source = new object();
            var errors = new ItemOrListEditor<object>(new List<object>());
            ItemOrIReadOnlyList<string> memberName = "test";
            var validator = GetComponentOwner(ComponentCollectionManager);
            validator.GetErrors(memberName, ref errors, null, DefaultMetadata);
            errors.Count.ShouldEqual(0);

            for (var i = 0; i < componentCount; i++)
            {
                var s = i.ToString();
                var component = new TestValidatorErrorManagerComponent(validator)
                {
                    GetErrorsRaw = (ItemOrIReadOnlyList<string> members, ref ItemOrListEditor<object> editor, object? src, IReadOnlyMetadataContext? metadata) =>
                    {
                        src.ShouldEqual(source);
                        members.ShouldEqual(memberName);
                        metadata.ShouldEqual(DefaultMetadata);
                        editor.Add(s);
                    }
                };
                validator.AddComponent(component);
            }

            validator.GetErrors(memberName, ref errors, source, DefaultMetadata);
            errors.Count.ShouldEqual(componentCount);
            var list = errors.AsList();
            for (var i = 0; i < componentCount; i++)
                list.ShouldContain(i.ToString());
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void GetErrorsShouldBeHandledByComponents(int componentCount)
        {
            var source = new object();
            var errors = new ItemOrListEditor<ValidationErrorInfo>(new List<ValidationErrorInfo>());
            ItemOrIReadOnlyList<string> memberName = "test";
            var validator = GetComponentOwner(ComponentCollectionManager);
            validator.GetErrors(memberName, ref errors, null, DefaultMetadata);
            errors.Count.ShouldEqual(0);

            for (var i = 0; i < componentCount; i++)
            {
                var s = i.ToString();
                var component = new TestValidatorErrorManagerComponent(validator)
                {
                    GetErrors = (ItemOrIReadOnlyList<string> members, ref ItemOrListEditor<ValidationErrorInfo> editor, object? src, IReadOnlyMetadataContext? metadata) =>
                    {
                        src.ShouldEqual(source);
                        members.ShouldEqual(memberName);
                        metadata.ShouldEqual(DefaultMetadata);
                        editor.Add(new ValidationErrorInfo(this, s, s));
                    }
                };
                validator.AddComponent(component);
            }

            validator.GetErrors(memberName, ref errors, source, DefaultMetadata);
            errors.Count.ShouldEqual(componentCount);
            var list = errors.AsList();
            for (var i = 0; i < componentCount; i++)
                list.ShouldContain(new ValidationErrorInfo(this, i.ToString(), i.ToString()));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void SetErrorsShouldBeHandledByComponents(int componentCount)
        {
            var errors = new[] {new ValidationErrorInfo(this, "1", "1"), new ValidationErrorInfo(this, "2", "2")};
            var source = new object();
            var count = 0;
            var validator = GetComponentOwner(ComponentCollectionManager);

            for (var i = 0; i < componentCount; i++)
            {
                var component = new TestValidatorErrorManagerComponent(validator)
                {
                    SetErrors = (s, e, metadata) =>
                    {
                        ++count;
                        e.ShouldEqual(errors);
                        s.ShouldEqual(source);
                        metadata.ShouldEqual(DefaultMetadata);
                    }
                };
                validator.AddComponent(component);
            }

            validator.SetErrors(source, errors, DefaultMetadata);
            count.ShouldEqual(componentCount);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void ClearErrorsShouldBeHandledByComponents(int componentCount)
        {
            ItemOrIReadOnlyList<string> memberName = "test";
            var source = new object();
            var count = 0;
            var validator = GetComponentOwner(ComponentCollectionManager);

            for (var i = 0; i < componentCount; i++)
            {
                var component = new TestValidatorErrorManagerComponent(validator)
                {
                    ClearErrors = (m, s, metadata) =>
                    {
                        ++count;
                        m.ShouldEqual(memberName);
                        s.ShouldEqual(source);
                        metadata.ShouldEqual(DefaultMetadata);
                    }
                };
                validator.AddComponent(component);
            }

            validator.ClearErrors(memberName, source, DefaultMetadata);
            count.ShouldEqual(componentCount);
        }

        protected override Validator GetComponentOwner(IComponentCollectionManager? componentCollectionManager = null) => new(null, componentCollectionManager);
    }
}