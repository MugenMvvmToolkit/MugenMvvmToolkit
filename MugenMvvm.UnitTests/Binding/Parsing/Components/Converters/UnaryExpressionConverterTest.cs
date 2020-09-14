using System.Collections.Generic;
using System.Linq.Expressions;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Binding.Parsing;
using MugenMvvm.Binding.Parsing.Components.Converters;
using MugenMvvm.Binding.Parsing.Expressions;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Binding.Parsing.Components.Converters
{
    public class UnaryExpressionConverterTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void TryConvertShouldIgnoreNotUnaryExpression()
        {
            var component = new UnaryExpressionConverter();
            var ctx = new ExpressionConverterContext<Expression>();
            component.TryConvert(ctx, Expression.Constant("")).ShouldBeNull();
        }

        [Fact]
        public void TryConvertShouldIgnoreNotSupportUnaryExpression()
        {
            var component = new UnaryExpressionConverter();
            component.Mapping.Clear();
            var ctx = new ExpressionConverterContext<Expression>();
            component.TryConvert(ctx, Expression.MakeUnary(ExpressionType.Negate, Expression.Constant(1), null)).ShouldBeNull();
        }

        [Theory]
        [MemberData(nameof(GetData))]
        public void TryConvertShouldConvertUnaryExpression(ExpressionConverterContext<Expression> ctx, Expression expression, IExpressionNode result) =>
            new UnaryExpressionConverter().TryConvert(ctx, expression).ShouldEqual(result);

        public static IEnumerable<object?[]> GetData() =>
            new[]
            {
                GetUnary(ExpressionType.Negate, UnaryTokenType.Minus, 1),
                GetUnary(ExpressionType.UnaryPlus, UnaryTokenType.Plus, -1),
                GetUnary(ExpressionType.Not, UnaryTokenType.LogicalNegation, true),
                GetUnary(ExpressionType.Not, UnaryTokenType.BitwiseNegation, 1),
                GetUnary(ExpressionType.Convert, null, 0),
                GetUnary(ExpressionType.ConvertChecked, null, 0)
            };

        private static object[] GetUnary<T>(ExpressionType expressionType, UnaryTokenType? unaryTokenType, T value)
        {
            var context = new ExpressionConverterContext<Expression>();
            var operand = Expression.Constant(value);
            var operandExpr = ConstantExpressionNode.Get(value);
            context.SetExpression(operand, operandExpr);
            IExpressionNode result;
            if (unaryTokenType == null)
                result = operandExpr;
            else
                result = new UnaryExpressionNode(unaryTokenType, operandExpr);

            return new object[] {context, Expression.MakeUnary(expressionType, operand, unaryTokenType == null ? typeof(T) : null), result};
        }

        #endregion
    }
}