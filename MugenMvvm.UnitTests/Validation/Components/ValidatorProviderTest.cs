using MugenMvvm.Constants;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Validation;
using MugenMvvm.UnitTests.ViewModels.Internal;
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
            var component = new ValidatorProvider();
            component.Priority.ShouldEqual(ValidationComponentPriority.ValidatorProvider);
            var validator = component.TryGetValidator(null!, this, DefaultMetadata);
            validator.ShouldBeType<Validator>();
            validator.Components.Count.ShouldEqual(1);
            validator.Components.Get<CycleHandlerValidatorBehavior>().Count.ShouldEqual(1);
        }

        [Fact]
        public void TryGetValidatorShouldReturnValidator2()
        {
            var validator = new Validator();
            var component = new ValidatorProvider();
            component.Priority.ShouldEqual(ValidationComponentPriority.ValidatorProvider);
            component.TryGetValidator(null!, new ValidatorTarget(validator), DefaultMetadata).ShouldEqual(validator);
        }

        [Fact]
        public void TryGetValidatorShouldReturnValidator3()
        {
            var target = new TestViewModelBase();
            var component = new ValidatorProvider();
            component.Priority.ShouldEqual(ValidationComponentPriority.ValidatorProvider);
            var validator = component.TryGetValidator(null!, target, DefaultMetadata);
            validator.ShouldBeType<Validator>();
            validator.Components.Count.ShouldEqual(2);
            validator.Components.Get<CycleHandlerValidatorBehavior>().Count.ShouldEqual(1);
            validator.Components.Get<ObservableValidatorBehavior>().Count.ShouldEqual(1);
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