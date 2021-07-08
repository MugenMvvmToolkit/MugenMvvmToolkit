using System.Linq.Expressions;
using MugenMvvm.Bindings.Parsing.Components.Converters;
using MugenMvvm.Bindings.Parsing.Expressions;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Parsing.Components.Converters
{
    public class ConditionExpressionConverterTest : ExpressionConverterTestBase<ConditionExpressionConverter>
    {
        [Fact]
        public void TryConvertShouldConvertConditionExpression()
        {
            var condition = Expression.Constant(true);
            var ifTrue = Expression.Constant(1);
            var ifFalse = Expression.Constant(2);

            var result = new ConditionExpressionNode(ConstantExpressionNode.True, ConstantExpressionNode.Get(1), ConstantExpressionNode.Get(2));

            Context.SetExpression(condition, result.Condition);
            Context.SetExpression(ifTrue, result.IfTrue);
            Context.SetExpression(ifFalse, result.IfFalse);
            Converter.TryConvert(Context, Expression.Condition(condition, ifTrue, ifFalse)).ShouldEqual(result);
        }

        [Fact]
        public void TryConvertShouldIgnoreNotConditionExpression() => Converter.TryConvert(Context, Expression.Constant("")).ShouldBeNull();
    }
}