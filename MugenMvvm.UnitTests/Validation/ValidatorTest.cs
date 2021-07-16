using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MugenMvvm.Collections;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Validation;
using MugenMvvm.Metadata;
using MugenMvvm.Tests.Internal;
using MugenMvvm.Tests.Validation;
using MugenMvvm.UnitTests.Components;
using MugenMvvm.Validation;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Validation
{
    public class ValidatorTest : ComponentOwnerTestBase<Validator>
    {
        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void ClearErrorsShouldBeHandledByComponents(int componentCount)
        {
            ItemOrIReadOnlyList<string> memberName = "test";
            var source = new object();
            var count = 0;

            for (var i = 0; i < componentCount; i++)
            {
                var component = new TestValidatorErrorManagerComponent
                {
                    ClearErrors = (v, m, s, metadata) =>
                    {
                        ++count;
                        v.ShouldEqual(Validator);
                        m.ShouldEqual(memberName);
                        s.ShouldEqual(source);
                        metadata.ShouldEqual(Metadata);
                    }
                };
                Validator.AddComponent(component);
            }

            Validator.ClearErrors(memberName, source, Metadata);
            count.ShouldEqual(componentCount);
        }

        [Theory]
        [InlineData(1, true)]
        [InlineData(10, true)]
        [InlineData(1, false)]
        [InlineData(10, false)]
        public void DisposeShouldClearComponentsMetadataNotifyListeners(int count, bool canDispose)
        {
            var invokeCount = 0;
            Validator.IsDisposable.ShouldBeTrue();
            Validator.IsDisposable = canDispose;

            for (var i = 0; i < count; i++)
            {
                Validator.Components.Add(new TestDisposableComponent<IValidator>
                {
                    Dispose = (o, m) =>
                    {
                        o.ShouldEqual(Validator);
                        ++invokeCount;
                    }
                });
            }

            Validator.IsDisposed.ShouldBeFalse();
            Validator.Metadata.Set(MetadataContextKey.FromKey<object?>("t"), "");
            Validator.Dispose();
            if (canDispose)
            {
                Validator.IsDisposed.ShouldBeTrue();
                invokeCount.ShouldEqual(count);
                Validator.Components.Count.ShouldEqual(0);
                Validator.Metadata.Count.ShouldEqual(0);
            }
            else
            {
                Validator.IsDisposed.ShouldBeFalse();
                invokeCount.ShouldEqual(0);
                Validator.Components.Count.ShouldEqual(count);
            }
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void GetErrorsRawShouldBeHandledByComponents(int componentCount)
        {
            var source = new object();
            var errors = new ItemOrListEditor<object>(new List<object>());
            ItemOrIReadOnlyList<string> memberName = "test";
            Validator.GetErrors(memberName, ref errors, null, Metadata);
            errors.Count.ShouldEqual(0);

            for (var i = 0; i < componentCount; i++)
            {
                var s = i.ToString();
                var component = new TestValidatorErrorManagerComponent
                {
                    GetErrorsRaw = (IValidator v, ItemOrIReadOnlyList<string> members, ref ItemOrListEditor<object> editor, object? src, IReadOnlyMetadataContext? metadata) =>
                    {
                        v.ShouldEqual(Validator);
                        src.ShouldEqual(source);
                        members.ShouldEqual(memberName);
                        metadata.ShouldEqual(Metadata);
                        editor.Add(s);
                    }
                };
                Validator.AddComponent(component);
            }

            Validator.GetErrors(memberName, ref errors, source, Metadata);
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
            Validator.GetErrors(memberName, ref errors, null, Metadata);
            errors.Count.ShouldEqual(0);

            for (var i = 0; i < componentCount; i++)
            {
                var s = i.ToString();
                var component = new TestValidatorErrorManagerComponent
                {
                    GetErrors = (IValidator v, ItemOrIReadOnlyList<string> members, ref ItemOrListEditor<ValidationErrorInfo> editor, object? src,
                        IReadOnlyMetadataContext? metadata) =>
                    {
                        v.ShouldEqual(Validator);
                        src.ShouldEqual(source);
                        members.ShouldEqual(memberName);
                        metadata.ShouldEqual(Metadata);
                        editor.Add(new ValidationErrorInfo(this, s, s));
                    }
                };
                Validator.AddComponent(component);
            }

            Validator.GetErrors(memberName, ref errors, source, Metadata);
            errors.Count.ShouldEqual(componentCount);
            var list = errors.AsList();
            for (var i = 0; i < componentCount; i++)
                list.ShouldContain(new ValidationErrorInfo(this, i.ToString(), i.ToString()));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void HasErrorsShouldBeHandledByComponents(int componentCount)
        {
            ItemOrIReadOnlyList<string> expectedMember = new[] { "1", "2" };
            var source = new object();
            var count = 0;
            var hasErrors = false;
            Validator.HasErrors().ShouldBeFalse();
            for (var i = 0; i < componentCount; i++)
            {
                var component = new TestValidatorErrorManagerComponent
                {
                    HasErrors = (v, m, s, meta) =>
                    {
                        ++count;
                        v.ShouldEqual(Validator);
                        m.ShouldEqual(expectedMember);
                        s.ShouldEqual(source);
                        meta.ShouldEqual(Metadata);
                        return hasErrors;
                    },
                    Priority = -i
                };
                Validator.AddComponent(component);
            }

            Validator.HasErrors(expectedMember, source, Metadata).ShouldBeFalse();
            count.ShouldEqual(componentCount);

            count = 0;
            expectedMember = "t";
            Validator.HasErrors(expectedMember, source, Metadata).ShouldBeFalse();
            count.ShouldEqual(componentCount);

            count = 0;
            hasErrors = true;
            Validator.HasErrors(expectedMember, source, Metadata).ShouldBeTrue();
            count.ShouldEqual(1);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void SetErrorsShouldBeHandledByComponents(int componentCount)
        {
            var errors = new[] { new ValidationErrorInfo(this, "1", "1"), new ValidationErrorInfo(this, "2", "2") };
            var source = new object();
            var count = 0;

            for (var i = 0; i < componentCount; i++)
            {
                var component = new TestValidatorErrorManagerComponent
                {
                    SetErrors = (v, s, e, metadata) =>
                    {
                        ++count;
                        v.ShouldEqual(Validator);
                        e.ShouldEqual(errors);
                        s.ShouldEqual(source);
                        metadata.ShouldEqual(Metadata);
                    }
                };
                Validator.AddComponent(component);
            }

            Validator.SetErrors(source, errors, Metadata);
            count.ShouldEqual(componentCount);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void ValidateAsyncShouldBeHandledByComponents(int componentCount)
        {
            var memberName = "test";
            var count = 0;
            var tasks = new List<TaskCompletionSource<object>>();

            for (var i = 0; i < componentCount; i++)
            {
                var tcs = new TaskCompletionSource<object>();
                tasks.Add(tcs);
                var component = new TestValidationHandlerComponent
                {
                    TryValidateAsync = (v, m, token, metadata) =>
                    {
                        ++count;
                        v.ShouldEqual(Validator);
                        m.ShouldEqual(memberName);
                        token.ShouldEqual(DefaultCancellationToken);
                        metadata.ShouldEqual(Metadata);
                        return tcs.Task;
                    }
                };
                Validator.AddComponent(component);
            }

            var task = Validator.ValidateAsync(memberName, DefaultCancellationToken, Metadata);
            task.IsCompleted.ShouldBeFalse();

            for (var i = 0; i < componentCount - 1; i++)
                tasks[i].SetResult(i);
            task.IsCompleted.ShouldBeFalse();
            tasks.Last().SetResult("");
            task.IsCompleted.ShouldBeTrue();
            count.ShouldEqual(componentCount);
        }

        protected override Validator GetComponentOwner(IComponentCollectionManager? componentCollectionManager = null) => new(null, componentCollectionManager);
    }
}