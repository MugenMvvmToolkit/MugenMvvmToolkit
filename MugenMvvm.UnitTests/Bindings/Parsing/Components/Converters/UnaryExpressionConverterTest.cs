﻿using System.Collections.Generic;
using System.Linq.Expressions;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Bindings.Parsing;
using MugenMvvm.Bindings.Parsing.Components.Converters;
using MugenMvvm.Bindings.Parsing.Expressions;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Parsing.Components.Converters
{
    public class UnaryExpressionConverterTest : ExpressionConverterTestBase<UnaryExpressionConverter>
    {
        [Theory]
        [MemberData(nameof(GetData))]
        public void TryConvertShouldConvertUnaryExpression(ExpressionConverterContext<Expression> ctx, Expression expression, IExpressionNode result) =>
            Converter.TryConvert(ctx, expression).ShouldEqual(result);

        [Fact]
        public void TryConvertShouldIgnoreNotSupportUnaryExpression()
        {
            Converter.Mapping.Clear();
            Converter.TryConvert(Context, Expression.MakeUnary(ExpressionType.Negate, Expression.Constant(1), null!)).ShouldBeNull();
        }

        [Fact]
        public void TryConvertShouldIgnoreNotUnaryExpression() => Converter.TryConvert(Context, Expression.Constant("")).ShouldBeNull();

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

            return new object[] { context, Expression.MakeUnary(expressionType, operand, unaryTokenType == null ? typeof(T) : null!), result };
        }
    }
}