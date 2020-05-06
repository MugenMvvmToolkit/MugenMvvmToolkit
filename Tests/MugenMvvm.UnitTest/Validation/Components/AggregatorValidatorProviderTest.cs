using MugenMvvm.Validation;
using MugenMvvm.Validation.Components;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Validation.Components
{
    public class AggregatorValidatorProviderTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void TryGetAggregatorValidatorShouldReturnAggregatorValidator()
        {
            var component = new AggregatorValidatorProvider();
            component.TryGetAggregatorValidator(this, DefaultMetadata).ShouldBeType<AggregatorValidator>();
        }

        #endregion
    }
}