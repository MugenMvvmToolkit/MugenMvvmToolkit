using System.Linq.Expressions;
using MugenMvvm.Bindings.Parsing;
using MugenMvvm.Bindings.Parsing.Components.Converters;
using MugenMvvm.Bindings.Parsing.Expressions;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Parsing.Components.Converters
{
    public class ConstantExpressionConverterTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void TryConvertShouldIgnoreNotConstantExpression()
        {
            var component = new ConstantExpressionConverter();
            var ctx = new ExpressionConverterContext<Expression>();
            component.TryConvert(ctx, Expression.Parameter(typeof(object))).ShouldBeNull();
        }

        [Fact]
        public void TryConvertShouldConvertConstantExpression()
        {
            var component = new ConstantExpressionConverter();
            var ctx = new ExpressionConverterContext<Expression>();
            component.TryConvert(ctx, Expression.Constant(1)).ShouldEqual(ConstantExpressionNode.Get(1));
        }

        #endregion
    }
}