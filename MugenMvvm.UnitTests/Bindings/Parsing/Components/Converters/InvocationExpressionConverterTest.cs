using System;
using System.Linq.Expressions;
using MugenMvvm.Bindings.Parsing.Components.Converters;
using MugenMvvm.Bindings.Parsing.Expressions;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Parsing.Components.Converters
{
    public class InvocationExpressionConverterTest : ExpressionConverterTestBase<InvocationExpressionConverter>
    {
        [Fact]
        public void TryConvertShouldConvertInvocationExpression()
        {
            var action = new Action<int>(i => { });
            var expression = Expression.Invoke(Expression.Constant(action), Expression.Constant(1));
            var expectedResult = new MethodCallExpressionNode(ConstantExpressionNode.Get(action), nameof(action.Invoke), ConstantExpressionNode.Get(1));
            Context.SetExpression(expression.Expression, expectedResult.Target!);
            Context.SetExpression(expression.Arguments[0], expectedResult.Arguments[0]);
            Converter.TryConvert(Context, expression).ShouldEqual(expectedResult);
        }
    }
}