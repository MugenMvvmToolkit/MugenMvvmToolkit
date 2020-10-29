using MugenMvvm.Bindings.Compiling;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Compiling
{
    public class ParameterValueTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void IsEmptyShouldBeTrueDefault()
        {
            ParameterValue value = default;
            value.IsEmpty.ShouldBeTrue();
        }

        [Fact]
        public void ConstructorShouldInitializeValues1()
        {
            var type = typeof(object);
            var value = "";
            var expressionValue = new ParameterValue(typeof(object), value);
            expressionValue.IsEmpty.ShouldBeFalse();
            expressionValue.Type.ShouldEqual(type);
            expressionValue.Value.ShouldEqual(value);
        }

        #endregion
    }
}