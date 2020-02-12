using MugenMvvm.Validation;
using MugenMvvm.Validation.Components;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Validation
{
    public class AggregatorValidatorProviderComponentTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void TryGetAggregatorValidatorShouldReturnAggregatorValidator()
        {
            var component = new AggregatorValidatorProviderComponent();
            component.TryGetAggregatorValidator(this, DefaultMetadata).ShouldBeType<AggregatorValidator>();
        }

        #endregion
    }
}