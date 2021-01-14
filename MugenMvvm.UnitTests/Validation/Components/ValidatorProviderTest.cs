using MugenMvvm.Constants;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Validation;
using MugenMvvm.Validation;
using MugenMvvm.Validation.Components;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Validation.Components
{
    public class ValidatorProviderTest : UnitTestBase
    {
        [Fact]
        public void TryGetValidatorShouldReturnValidator1()
        {
            var component = new ValidatorProviderComponent();
            component.Priority.ShouldEqual(ValidationComponentPriority.ValidatorProvider);
            var validator = component.TryGetValidator(null!, this, DefaultMetadata);
            validator.ShouldBeType<Validator>();
            validator.Components.Count.ShouldEqual(1);
            validator.Components.Get<CycleHandlerValidatorComponent>().Count.ShouldEqual(1);
        }

        [Fact]
        public void TryGetValidatorShouldReturnValidator2()
        {
            var validator = new Validator();
            var component = new ValidatorProviderComponent();
            component.Priority.ShouldEqual(ValidationComponentPriority.ValidatorProvider);
            component.TryGetValidator(null!, new ValidatorTarget(validator), DefaultMetadata).ShouldEqual(validator);
        }

        private sealed class ValidatorTarget : IHasTarget<IValidator>
        {
            public ValidatorTarget(IValidator target)
            {
                Target = target;
            }

            public IValidator Target { get; }
        }
    }
}