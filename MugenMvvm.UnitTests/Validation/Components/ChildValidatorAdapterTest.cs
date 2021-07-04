using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MugenMvvm.Collections;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Validation;
using MugenMvvm.Interfaces.Validation.Components;
using MugenMvvm.Tests.Internal;
using MugenMvvm.Tests.Validation;
using MugenMvvm.Validation;
using MugenMvvm.Validation.Components;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Validation.Components
{
    public class ChildValidatorAdapterTest : UnitTestBase
    {
        private readonly ChildValidatorAdapter _adapter;

        public ChildValidatorAdapterTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _adapter = new ChildValidatorAdapter();
            Validator.AddComponent(_adapter);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void AddRemoveValidatorShouldRaiseErrorsChanged(bool extMethod)
        {
            var validator = new Validator(null, ComponentCollectionManager);
            var invokeCount = 0;
            Validator.AddComponent(new TestValidatorErrorsChangedListener
            {
                OnErrorsChanged = (v, e, _) =>
                {
                    v.ShouldEqual(Validator);
                    e.IsEmpty.ShouldBeTrue();
                    ++invokeCount;
                }
            });

            if (extMethod)
                Validator.AddChildValidator(validator);
            else
                _adapter.Add(validator);
            _adapter.Contains(validator).ShouldBeTrue();
            invokeCount.ShouldEqual(1);

            if (extMethod)
                Validator.RemoveChildValidator(validator);
            else
                _adapter.Remove(validator);
            _adapter.Contains(validator).ShouldBeFalse();
            invokeCount.ShouldEqual(2);
        }

        [Theory]
        [InlineData(1, true)]
        [InlineData(10, true)]
        [InlineData(1, false)]
        [InlineData(10, false)]
        public void DisposeShouldDisposeChildValidators(int count, bool canDispose)
        {
            var invokeCount = 0;
            _adapter.DisposeChildValidators = canDispose;

            for (var i = 0; i < count; i++)
            {
                var validator = new Validator(null, ComponentCollectionManager);
                validator.AddComponent(new TestDisposableComponent<IValidator>
                {
                    Dispose = (o, _) =>
                    {
                        o.ShouldEqual(validator);
                        ++invokeCount;
                    }
                });
                Validator.AddChildValidator(validator);
            }

            Validator.Dispose();
            invokeCount.ShouldEqual(canDispose ? count : 0);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void GetErrorsRawShouldBeHandledByComponents(int validatorCount)
        {
            var source = new object();
            var errors = new ItemOrListEditor<object>(new List<object>());
            ItemOrIReadOnlyList<string> memberName = "test";
            Validator.GetErrors(memberName, ref errors, null, DefaultMetadata);
            errors.Count.ShouldEqual(0);

            for (var i = 0; i < validatorCount; i++)
            {
                var s = i.ToString();
                var validator = new Validator(null, ComponentCollectionManager);
                validator.AddComponent(new TestValidatorErrorManagerComponent
                {
                    GetErrorsRaw = (IValidator v, ItemOrIReadOnlyList<string> members, ref ItemOrListEditor<object> editor, object? src, IReadOnlyMetadataContext? metadata) =>
                    {
                        v.ShouldEqual(validator);
                        src.ShouldEqual(source);
                        members.ShouldEqual(memberName);
                        metadata.ShouldEqual(DefaultMetadata);
                        editor.Add(s);
                    }
                });
                Validator.AddChildValidator(validator);
            }

            Validator.GetErrors(memberName, ref errors, source, DefaultMetadata);
            errors.Count.ShouldEqual(validatorCount);
            var list = errors.AsList();
            for (var i = 0; i < validatorCount; i++)
                list.ShouldContain(i.ToString());
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void GetErrorsShouldBeHandledByComponents(int validatorCount)
        {
            var source = new object();
            var errors = new ItemOrListEditor<ValidationErrorInfo>(new List<ValidationErrorInfo>());
            ItemOrIReadOnlyList<string> memberName = "test";
            Validator.GetErrors(memberName, ref errors, null, DefaultMetadata);
            errors.Count.ShouldEqual(0);

            for (var i = 0; i < validatorCount; i++)
            {
                var s = i.ToString();
                var validator = new Validator(null, ComponentCollectionManager);
                validator.AddComponent(new TestValidatorErrorManagerComponent
                {
                    GetErrors = (IValidator v, ItemOrIReadOnlyList<string> members, ref ItemOrListEditor<ValidationErrorInfo> editor, object? src,
                        IReadOnlyMetadataContext? metadata) =>
                    {
                        v.ShouldEqual(validator);
                        src.ShouldEqual(source);
                        members.ShouldEqual(memberName);
                        metadata.ShouldEqual(DefaultMetadata);
                        editor.Add(new ValidationErrorInfo(this, s, s));
                    }
                });
                Validator.AddChildValidator(validator);
            }

            Validator.GetErrors(memberName, ref errors, source, DefaultMetadata);
            errors.Count.ShouldEqual(validatorCount);
            var list = errors.AsList();
            for (var i = 0; i < validatorCount; i++)
                list.ShouldContain(new ValidationErrorInfo(this, i.ToString(), i.ToString()));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void HasErrorsShouldBeHandledByComponents(int validatorCount)
        {
            ItemOrIReadOnlyList<string> expectedMember = new[] { "1", "2" };
            var source = new object();
            var count = 0;
            var hasErrors = false;
            Validator.HasErrors().ShouldBeFalse();
            for (var i = 0; i < validatorCount; i++)
            {
                var validator = new Validator(null, ComponentCollectionManager);
                validator.AddComponent(new TestValidatorErrorManagerComponent
                {
                    HasErrors = (v, m, s, meta) =>
                    {
                        ++count;
                        v.ShouldEqual(validator);
                        m.ShouldEqual(expectedMember);
                        s.ShouldEqual(source);
                        meta.ShouldEqual(DefaultMetadata);
                        return hasErrors;
                    },
                    Priority = -i
                });
                Validator.AddChildValidator(validator);
            }

            Validator.HasErrors(expectedMember, source, DefaultMetadata).ShouldBeFalse();
            count.ShouldEqual(validatorCount);

            count = 0;
            expectedMember = "t";
            Validator.HasErrors(expectedMember, source, DefaultMetadata).ShouldBeFalse();
            count.ShouldEqual(validatorCount);

            count = 0;
            hasErrors = true;
            Validator.HasErrors(expectedMember, source, DefaultMetadata).ShouldBeTrue();
            count.ShouldEqual(1);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void ShouldListenChildAsyncValidationEvents(int validatorCount)
        {
            var count = 0;
            var validators = new List<Validator>();
            var member = NewId();
            var task = Task.CompletedTask;
            for (var i = 0; i < validatorCount; i++)
            {
                var validator = new Validator(null, ComponentCollectionManager);
                validators.Add(validator);
                Validator.AddChildValidator(validator);
            }

            Validator.AddComponent(new TestAsyncValidationListener
            {
                OnAsyncValidation = (v, e, t, m) =>
                {
                    v.ShouldEqual(Validator);
                    e.ShouldEqual(member);
                    t.ShouldEqual(task);
                    m.ShouldEqual(DefaultMetadata);
                    ++count;
                }
            });

            foreach (var validator in validators)
                validator.GetComponents<IAsyncValidationListener>().OnAsyncValidation(validator, member, task, DefaultMetadata);
            count.ShouldEqual(validatorCount);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void ShouldListenChildErrorsChangedEvents(int validatorCount)
        {
            var count = 0;
            var validators = new List<Validator>();
            var member = NewId();
            for (var i = 0; i < validatorCount; i++)
            {
                var validator = new Validator(null, ComponentCollectionManager);
                validators.Add(validator);
                Validator.AddChildValidator(validator);
            }

            Validator.AddComponent(new TestValidatorErrorsChangedListener
            {
                OnErrorsChanged = (v, e, m) =>
                {
                    v.ShouldEqual(Validator);
                    e.ShouldEqual(member);
                    m.ShouldEqual(DefaultMetadata);
                    ++count;
                }
            });

            foreach (var validator in validators)
                validator.GetComponents<IValidatorErrorsChangedListener>().OnErrorsChanged(validator, member, DefaultMetadata);
            count.ShouldEqual(validatorCount);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void ValidateAsyncShouldBeHandledByValidators(int validatorCount)
        {
            var memberName = "test";
            var count = 0;
            var tasks = new List<TaskCompletionSource<object>>();

            for (var i = 0; i < validatorCount; i++)
            {
                var tcs = new TaskCompletionSource<object>();
                tasks.Add(tcs);
                var validator = new Validator(null, ComponentCollectionManager);
                validator.AddComponent(new TestValidationHandlerComponent
                {
                    TryValidateAsync = (v, m, token, metadata) =>
                    {
                        ++count;
                        v.ShouldEqual(validator);
                        m.ShouldEqual(memberName);
                        token.ShouldEqual(DefaultCancellationToken);
                        metadata.ShouldEqual(DefaultMetadata);
                        return tcs.Task;
                    }
                });
                Validator.AddChildValidator(validator);
            }

            var task = Validator.ValidateAsync(memberName, DefaultCancellationToken, DefaultMetadata);
            task.IsCompleted.ShouldBeFalse();

            for (var i = 0; i < validatorCount - 1; i++)
                tasks[i].SetResult(i);
            task.IsCompleted.ShouldBeFalse();
            tasks.Last().SetResult("");
            task.IsCompleted.ShouldBeTrue();
            count.ShouldEqual(validatorCount);
        }
    }
}