using MugenMvvm.Collections;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Validation;
using MugenMvvm.Interfaces.Validation.Components;
using MugenMvvm.Tests.Validation;
using MugenMvvm.Validation;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Validation
{
    public class NotifyDataErrorInfoValidatorAdapterTest : UnitTestBase
    {
        private readonly NotifyDataErrorInfoValidatorAdapter _adapter;

        public NotifyDataErrorInfoValidatorAdapterTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _adapter = new NotifyDataErrorInfoValidatorAdapter(Validator, this);
        }

        [Fact]
        public void ErrorsChangedShouldListenOnErrorsChanged()
        {
            var invokeCount = 0;
            var propertyName = "Test";

            _adapter.ErrorsChanged += (sender, args) =>
            {
                sender.ShouldEqual(this);
                args.PropertyName.ShouldEqual(propertyName);
                ++invokeCount;
            };

            Validator.GetComponents<IValidatorErrorsChangedListener>().OnErrorsChanged(Validator, propertyName, DefaultMetadata);
            invokeCount.ShouldEqual(1);

            Validator.Dispose();
            Validator.GetComponents<IValidatorErrorsChangedListener>().OnErrorsChanged(Validator, propertyName, DefaultMetadata);
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public void GetErrorsShouldUseValidator()
        {
            var propertyName = "Test";
            var errors = new[] { "1", "2" };

            Validator.AddComponent(new TestValidatorErrorManagerComponent
            {
                GetErrorsRaw = (IValidator v, ItemOrIReadOnlyList<string> members, ref ItemOrListEditor<object> editor, object? source, IReadOnlyMetadataContext? metadata) =>
                {
                    v.ShouldEqual(Validator);
                    members.Item.ShouldEqual(propertyName);
                    source.ShouldBeNull();
                    editor.AddRange(value: errors);
                }
            });
            _adapter.GetErrors(propertyName).ShouldEqual(errors);
        }

        [Fact]
        public void HasErrorsShouldUseValidator()
        {
            var hasErrors = false;
            Validator.AddComponent(new TestValidatorErrorManagerComponent
            {
                HasErrors = (vv, v, s, _) =>
                {
                    vv.ShouldEqual(Validator);
                    v.IsEmpty.ShouldBeTrue();
                    s.ShouldBeNull();
                    return hasErrors;
                }
            });

            _adapter.HasErrors.ShouldEqual(hasErrors);
            hasErrors = true;
            _adapter.HasErrors.ShouldEqual(hasErrors);
        }
    }
}