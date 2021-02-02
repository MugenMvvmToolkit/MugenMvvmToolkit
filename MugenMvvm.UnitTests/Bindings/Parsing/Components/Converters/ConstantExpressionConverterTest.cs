using System.Linq.Expressions;
using MugenMvvm.Bindings.Parsing.Components.Converters;
using MugenMvvm.Bindings.Parsing.Expressions;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Parsing.Components.Converters
{
    public class ConstantExpressionConverterTest : ExpressionConverterTestBase<ConstantExpressionConverter>
    {
        [Fact]
        public void TryConvertShouldConvertConstantExpression() => Converter.TryConvert(Context, Expression.Constant(1)).ShouldEqual(ConstantExpressionNode.Get(1));

        [Fact]
        public void TryConvertShouldIgnoreNotConstantExpression() => Converter.TryConvert(Context, Expression.Parameter(typeof(object))).ShouldBeNull();
    }
}