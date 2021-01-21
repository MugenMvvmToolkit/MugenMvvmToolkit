using MugenMvvm.Constants;
using MugenMvvm.UnitTests.ViewModels.Internal;
using MugenMvvm.Validation;
using MugenMvvm.Validation.Components;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Validation.Components
{
    public class ValidatorProviderTest : UnitTestBase
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void TryGetValidatorShouldReturnValidator1(bool isAsync)
        {
            var component = new ValidatorProvider {AsyncValidationEnabled = isAsync};
            component.Priority.ShouldEqual(ValidationComponentPriority.ValidatorProvider);
            var validator = component.TryGetValidator(null!, this, DefaultMetadata);
            validator.ShouldBeType<Validator>();
            validator.Components.Count.ShouldEqual(isAsync ? 3 : 2);
            validator.Components.Get<CycleHandlerValidatorBehavior>().Count.ShouldEqual(1);
            validator.Components.Get<ValidatorErrorManager>().Count.ShouldEqual(1);
            if (isAsync)
                validator.Components.Get<AsyncValidationBehavior>().Count.ShouldEqual(1);
        }

        [Fact]
        public void TryGetValidatorShouldReturnValidator2()
        {
            var target = new TestViewModelBase();
            var component = new ValidatorProvider();
            component.Priority.ShouldEqual(ValidationComponentPriority.ValidatorProvider);
            var validator = component.TryGetValidator(null!, target, DefaultMetadata);
            validator.ShouldBeType<Validator>();
            validator.Components.Count.ShouldEqual(3);
            validator.Components.Get<CycleHandlerValidatorBehavior>().Count.ShouldEqual(1);
            validator.Components.Get<ObservableValidatorBehavior>().Count.ShouldEqual(1);
            validator.Components.Get<ValidatorErrorManager>().Count.ShouldEqual(1);
        }
    }
}