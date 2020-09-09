using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Validation.Components;
using MugenMvvm.UnitTest.Validation.Internal;
using MugenMvvm.Validation;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Validation
{
    public class NotifyDataErrorInfoValidatorAdapterTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void HasErrorsShouldUseValidator()
        {
            var hasErrors = false;
            var validator = new Validator();
            validator.AddComponent(new TestValidatorComponent
            {
                HasErrors = (v, s, arg3) =>
                {
                    s.ShouldBeNull();
                    return hasErrors;
                }
            });

            var adapter = new NotifyDataErrorInfoValidatorAdapter(validator);
            adapter.HasErrors.ShouldEqual(hasErrors);

            hasErrors = true;
            adapter.HasErrors.ShouldEqual(hasErrors);
        }

        [Fact]
        public void GetErrorsShouldUseValidator()
        {
            var propertyName = "Test";
            var errors = new[] {"1", "2"};
            var validator = new Validator();
            validator.AddComponent(new TestValidatorComponent
            {
                GetErrors = (v, s, arg3) =>
                {
                    s.ShouldEqual(propertyName);
                    return errors;
                }
            });

            var adapter = new NotifyDataErrorInfoValidatorAdapter(validator);
            adapter.GetErrors(propertyName).ShouldEqual(errors);
        }

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

            validator.GetComponents<IValidatorListener>().OnErrorsChanged(validator, this, propertyName, DefaultMetadata);
            invokeCount.ShouldEqual(1);

            validator.Dispose();
            validator.GetComponents<IValidatorListener>().OnErrorsChanged(validator, this, propertyName, DefaultMetadata);
            invokeCount.ShouldEqual(1);
        }

        #endregion
    }
}