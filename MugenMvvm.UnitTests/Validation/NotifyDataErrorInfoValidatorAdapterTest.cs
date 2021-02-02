using MugenMvvm.Collections;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Validation.Components;
using MugenMvvm.UnitTests.Validation.Internal;
using MugenMvvm.Validation;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Validation
{
    public class NotifyDataErrorInfoValidatorAdapterTest : UnitTestBase
    {
        private readonly Validator _validator;
        private readonly NotifyDataErrorInfoValidatorAdapter _adapter;

        public NotifyDataErrorInfoValidatorAdapterTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _validator = new Validator(null, ComponentCollectionManager);
            _adapter = new NotifyDataErrorInfoValidatorAdapter(_validator, this);
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

            _validator.GetComponents<IValidatorErrorsChangedListener>().OnErrorsChanged(_validator, propertyName, DefaultMetadata);
            invokeCount.ShouldEqual(1);

            _validator.Dispose();
            _validator.GetComponents<IValidatorErrorsChangedListener>().OnErrorsChanged(_validator, propertyName, DefaultMetadata);
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public void GetErrorsShouldUseValidator()
        {
            var propertyName = "Test";
            var errors = new[] {"1", "2"};

            _validator.AddComponent(new TestValidatorErrorManagerComponent(_validator)
            {
                GetErrorsRaw = (ItemOrIReadOnlyList<string> members, ref ItemOrListEditor<object> editor, object? source, IReadOnlyMetadataContext? metadata) =>
                {
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
            _validator.AddComponent(new TestValidatorErrorManagerComponent(_validator)
            {
                HasErrors = (v, s, _) =>
                {
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