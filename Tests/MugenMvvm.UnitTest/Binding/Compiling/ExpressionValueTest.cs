using MugenMvvm.Binding.Compiling;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Compiling
{
    public class ExpressionValueTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void IsEmptyShouldBeTrueDefault()
        {
            ExpressionValue value = default;
            value.IsEmpty.ShouldBeTrue();
        }

        [Fact]
        public void HasValueShouldBeFalseDefault()
        {
            ExpressionValue value = default;
            value.HasValue.ShouldBeFalse();
        }

        [Fact]
        public void ConstructorShouldInitializeValues1()
        {
            var type = typeof(object);
            var value = "";
            var expressionValue = new ExpressionValue(typeof(object), value);
            expressionValue.HasValue.ShouldBeTrue();
            expressionValue.IsEmpty.ShouldBeFalse();
            expressionValue.Type.ShouldEqual(type);
            expressionValue.Value.ShouldEqual(value);
        }

        #endregion
    }
}