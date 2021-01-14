using System.Collections.Generic;
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
    public class BinaryExpressionConverterTest : UnitTestBase
    {
        [Fact]
        public void TryConvertShouldIgnoreNotBinaryExpression()
        {
            var component = new BinaryExpressionConverter();
            var ctx = new ExpressionConverterContext<Expression>();
            component.TryConvert(ctx, Expression.Constant("")).ShouldBeNull();
        }

        [Fact]
        public void TryConvertShouldIgnoreNotSupportBinaryExpression()
        {
            var component = new BinaryExpressionConverter();
            component.Mapping.Clear();
            var ctx = new ExpressionConverterContext<Expression>();
            component.TryConvert(ctx, Expression.MakeBinary(ExpressionType.Add, Expression.Constant(1), Expression.Constant(1))).ShouldBeNull();
        }

        [Theory]
        [MemberData(nameof(GetData))]
        public void TryConvertShouldConvertBinaryExpression(ExpressionConverterContext<Expression> ctx, Expression expression, IExpressionNode result) =>
            new BinaryExpressionConverter().TryConvert(ctx, expression).ShouldEqual(result);

        public static IEnumerable<object?[]> GetData() =>
            new[]
            {
                GetBinary(ExpressionType.Multiply, BinaryTokenType.Multiplication),
                GetBinary(ExpressionType.Divide, BinaryTokenType.Division),
                GetBinary(ExpressionType.Modulo, BinaryTokenType.Remainder),
                GetBinary(ExpressionType.Add, BinaryTokenType.Addition),
                GetBinary(ExpressionType.Subtract, BinaryTokenType.Subtraction),
                GetBinary(ExpressionType.LeftShift, BinaryTokenType.LeftShift),
                GetBinary(ExpressionType.RightShift, BinaryTokenType.RightShift),
                GetBinary(ExpressionType.LessThan, BinaryTokenType.LessThan),
                GetBinary(ExpressionType.GreaterThan, BinaryTokenType.GreaterThan),
                GetBinary(ExpressionType.LessThanOrEqual, BinaryTokenType.LessThanOrEqual),
                GetBinary(ExpressionType.GreaterThanOrEqual, BinaryTokenType.GreaterThanOrEqual),
                GetBinary(ExpressionType.Equal, BinaryTokenType.Equality),
                GetBinary(ExpressionType.NotEqual, BinaryTokenType.NotEqual),
                GetBinary(ExpressionType.And, BinaryTokenType.LogicalAnd),
                GetBinary(ExpressionType.ExclusiveOr, BinaryTokenType.LogicalXor),
                GetBinary(ExpressionType.Or, BinaryTokenType.LogicalOr),
                GetBinary(ExpressionType.AndAlso, BinaryTokenType.ConditionalAnd, true, false),
                GetBinary(ExpressionType.OrElse, BinaryTokenType.ConditionalOr, true, false),
                GetBinary(ExpressionType.Coalesce, BinaryTokenType.NullCoalescing, null!, "")
            };

        private static object[] GetBinary(ExpressionType expressionType, BinaryTokenType binaryTokenType) => GetBinary(expressionType, binaryTokenType, 1, 2);

        private static object[] GetBinary<T>(ExpressionType expressionType, BinaryTokenType binaryTokenType, T v1, T v2)
        {
            var context = new ExpressionConverterContext<Expression>();
            var left = Expression.Constant(v1);
            var right = Expression.Constant(v2);
            var result = new BinaryExpressionNode(binaryTokenType, ConstantExpressionNode.Get(1), ConstantExpressionNode.Get(2));
            context.SetExpression(left, result.Left);
            context.SetExpression(right, result.Right);
            return new object[] {context, Expression.MakeBinary(expressionType, left, right), result};
        }
    }
}