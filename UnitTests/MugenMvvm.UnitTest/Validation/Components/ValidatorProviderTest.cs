using MugenMvvm.Constants;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Validation;
using MugenMvvm.Validation;
using MugenMvvm.Validation.Components;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Validation.Components
{
    public class ValidatorProviderTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void TryGetValidatorShouldReturnValidator1()
        {
            var component = new ValidatorProviderComponent();
            component.Priority.ShouldEqual(ValidationComponentPriority.ValidatorProvider);
            component.TryGetValidator(null!, this, DefaultMetadata).ShouldBeType<Validator>();
        }

        [Fact]
        public void TryGetValidatorShouldReturnValidator2()
        {
            var validator = new Validator();
            var component = new ValidatorProviderComponent();
            component.Priority.ShouldEqual(ValidationComponentPriority.ValidatorProvider);
            component.TryGetValidator(null!, new ValidatorTarget(validator), DefaultMetadata).ShouldEqual(validator);
        }

        #endregion

        #region Nested types

        private sealed class ValidatorTarget : IHasTarget<IValidator>
        {
            #region Constructors

            public ValidatorTarget(IValidator target)
            {
                Target = target;
            }

            #endregion

            #region Properties

            public IValidator Target { get; }

            #endregion
        }

        #endregion
    }
}