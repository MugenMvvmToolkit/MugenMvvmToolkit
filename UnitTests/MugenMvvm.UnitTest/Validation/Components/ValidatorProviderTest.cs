using MugenMvvm.Constants;
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
        public void TryGetAggregatorValidatorShouldReturnAggregatorValidator()
        {
            var component = new ValidatorProviderComponent();
            component.Priority.ShouldEqual(ValidationComponentPriority.ValidatorProvider);
            component.TryGetValidator(this, DefaultMetadata).ShouldBeType<Validator>();
        }

        #endregion
    }
}