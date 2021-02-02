using System.Linq.Expressions;
using MugenMvvm.Bindings.Parsing.Components.Converters;
using MugenMvvm.Bindings.Parsing.Expressions;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Parsing.Components.Converters
{
    public class DefaultExpressionConverterTest : ExpressionConverterTestBase<DefaultExpressionConverter>
    {
        [Fact]
        public void TryConvertShouldConvertConstantExpression() => Converter.TryConvert(Context, Expression.Default(typeof(int))).ShouldEqual(ConstantExpressionNode.Get(0));

        [Fact]
        public void TryConvertShouldIgnoreNotDefaultExpression() => Converter.TryConvert(Context, Expression.Parameter(typeof(object))).ShouldBeNull();
    }
}