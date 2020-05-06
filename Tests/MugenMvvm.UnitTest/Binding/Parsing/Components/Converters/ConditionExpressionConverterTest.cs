using System.Linq.Expressions;
using MugenMvvm.Binding.Parsing;
using MugenMvvm.Binding.Parsing.Components.Converters;
using MugenMvvm.Binding.Parsing.Expressions;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Parsing.Components.Converters
{
    public class ConditionExpressionConverterTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void TryConvertShouldIgnoreNotConditionExpression()
        {
            var component = new ConditionExpressionConverter();
            var ctx = new ExpressionConverterContext<Expression>();
            component.TryConvert(ctx, Expression.Constant("")).ShouldBeNull();
        }

        [Fact]
        public void TryConvertShouldConvertConditionExpression()
        {
            var condition = Expression.Constant(true);
            var ifTrue = Expression.Constant(1);
            var ifFalse = Expression.Constant(2);

            var result = new ConditionExpressionNode(ConstantExpressionNode.True, ConstantExpressionNode.Get(1), ConstantExpressionNode.Get(2));
            var ctx = new ExpressionConverterContext<Expression>();
            ctx.SetExpression(condition, result.Condition);
            ctx.SetExpression(ifTrue, result.IfTrue);
            ctx.SetExpression(ifFalse, result.IfFalse);

            var component = new ConditionExpressionConverter();
            component.TryConvert(ctx, Expression.Condition(condition, ifTrue, ifFalse)).EqualsEx(result).ShouldBeTrue();
        }

        #endregion
    }
}