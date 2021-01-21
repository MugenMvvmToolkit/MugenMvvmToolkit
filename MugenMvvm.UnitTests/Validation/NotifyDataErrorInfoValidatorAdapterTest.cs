using MugenMvvm.Collections;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Validation.Components;
using MugenMvvm.UnitTests.Validation.Internal;
using MugenMvvm.Validation;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Validation
{
    public class NotifyDataErrorInfoValidatorAdapterTest : UnitTestBase
    {
        [Fact]
        public void ErrorsChangedShouldListenOnErrorsChanged()
        {
            var invokeCount = 0;
            var propertyName = "Test";
            var validator = new Validator();
            var adapter = new NotifyDataErrorInfoValidatorAdapter(validator, this);
            adapter.ErrorsChanged += (sender, args) =>
            {
                sender.ShouldEqual(this);
                args.PropertyName.ShouldEqual(propertyName);
                ++invokeCount;
            };

            validator.GetComponents<IValidatorErrorsChangedListener>().OnErrorsChanged(validator, propertyName, DefaultMetadata);
            invokeCount.ShouldEqual(1);

            validator.Dispose();
            validator.GetComponents<IValidatorErrorsChangedListener>().OnErrorsChanged(validator, propertyName, DefaultMetadata);
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public void GetErrorsShouldUseValidator()
        {
            var propertyName = "Test";
            var errors = new[] {"1", "2"};
            var validator = new Validator();
            validator.AddComponent(new TestValidatorErrorManagerComponent(validator)
            {
                GetErrorsRaw = (ItemOrIReadOnlyList<string> members, ref ItemOrListEditor<object> editor, object? source, IReadOnlyMetadataContext? metadata) =>
                {
                    members.Item.ShouldEqual(propertyName);
                    source.ShouldBeNull();
                    editor.AddRange(value: errors);
                }
            });

            var adapter = new NotifyDataErrorInfoValidatorAdapter(validator);
            adapter.GetErrors(propertyName).ShouldEqual(errors);
        }

        [Fact]
        public void HasErrorsShouldUseValidator()
        {
            var hasErrors = false;
            var validator = new Validator();
            validator.AddComponent(new TestValidatorErrorManagerComponent(validator)
            {
                HasErrors = (v, s, _) =>
                {
                    v.IsEmpty.ShouldBeTrue();
                    s.ShouldBeNull();
                    return hasErrors;
                }
            });

            var adapter = new NotifyDataErrorInfoValidatorAdapter(validator);
            adapter.HasErrors.ShouldEqual(hasErrors);

            hasErrors = true;
            adapter.HasErrors.ShouldEqual(hasErrors);
        }
    }
}