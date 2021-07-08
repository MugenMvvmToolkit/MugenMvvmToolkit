using System.Collections.Generic;
using MugenMvvm.Bindings.Compiling.Components;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Bindings.Parsing.Expressions;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Compiling.Components
{
    public class BinaryExpressionBuilderTest : ExpressionBuilderTestBase<BinaryExpressionBuilder>
    {
        [Theory]
        [MemberData(nameof(GetData))]
        public void TryBuildShouldBuildBinaryExpression(IBinaryExpressionNode binaryExpression, object result, bool invalid)
        {
            if (invalid)
            {
                ShouldThrow(() => Builder.TryBuild(Context, binaryExpression));
                return;
            }

            var expression = Builder.TryBuild(Context, binaryExpression)!;
            expression.ShouldNotBeNull();
            expression.Invoke().ShouldEqual(result);
        }

        [Fact]
        public void TryBuildShouldIgnoreNotBinaryExpression() => Builder.TryBuild(Context, ConstantExpressionNode.False).ShouldBeNull();

        [Fact]
        public void TryBuildShouldIgnoreNotSupportBinaryExpression()
        {
            Builder.Mapping.Clear();
            Builder.TryBuild(Context, new BinaryExpressionNode(BinaryTokenType.LogicalOr, ConstantExpressionNode.False, ConstantExpressionNode.False)).ShouldBeNull();
        }

        public static IEnumerable<object?[]> GetData() =>
            new[]
            {
                GetBinary(BinaryTokenType.Multiplication, 5, 10, 5 * 10, false),
                GetBinary(BinaryTokenType.Multiplication, 5.1f, 10, 5.1f * 10, false),
                GetBinary(BinaryTokenType.Multiplication, 5.1f, "t", null, true),

                GetBinary(BinaryTokenType.Division, 5, 10, 5 / 10, false),
                GetBinary(BinaryTokenType.Division, 5.1f, 10, 5.1f / 10, false),
                GetBinary(BinaryTokenType.Division, 5.1f, "t", null, true),

                GetBinary(BinaryTokenType.Remainder, 5, 10, 5 % 10, false),
                GetBinary(BinaryTokenType.Remainder, 5.1f, 10, 5.1f % 10, false),
                GetBinary(BinaryTokenType.Remainder, 5.1f, "t", null, true),

                GetBinary(BinaryTokenType.Addition, 5, 10, 5 + 10, false),
                GetBinary(BinaryTokenType.Addition, 5.1f, 10, 5.1f + 10, false),
                GetBinary(BinaryTokenType.Addition, 5.1f, "t", 5.1f + "t", false),
                GetBinary(BinaryTokenType.Addition, "t", 5.1f, "t" + 5.1f, false),
                GetBinary(BinaryTokenType.Addition, "t", "1", "t" + "1", false),

                GetBinary(BinaryTokenType.Subtraction, 5, 10, 5 - 10, false),
                GetBinary(BinaryTokenType.Subtraction, 5.1f, 10, 5.1f - 10, false),
                GetBinary(BinaryTokenType.Subtraction, 5.1f, "t", null, true),

                GetBinary(BinaryTokenType.LeftShift, 5, 10, 5 << 10, false),
                GetBinary(BinaryTokenType.LeftShift, 5.1f, 10, null, true),
                GetBinary(BinaryTokenType.LeftShift, 5.1f, "t", null, true),

                GetBinary(BinaryTokenType.RightShift, 5, 10, 5 >> 10, false),
                GetBinary(BinaryTokenType.RightShift, 5.1f, 10, null, true),
                GetBinary(BinaryTokenType.RightShift, 5.1f, "t", null, true),

                GetBinary(BinaryTokenType.LessThan, 5, 10, 5 < 10, false),
                GetBinary(BinaryTokenType.LessThan, 5.1f, 10, 5.1f < 10, false),
                GetBinary(BinaryTokenType.LessThan, 5.1f, "t", null, true),

                GetBinary(BinaryTokenType.GreaterThan, 5, 10, 5 > 10, false),
                GetBinary(BinaryTokenType.GreaterThan, 5.1f, 10, 5.1f > 10, false),
                GetBinary(BinaryTokenType.GreaterThan, 5.1f, "t", null, true),

                GetBinary(BinaryTokenType.LessThanOrEqual, 5, 10, 5 <= 10, false),
                GetBinary(BinaryTokenType.LessThanOrEqual, 5.1f, 10, 5.1f <= 10, false),
                GetBinary(BinaryTokenType.LessThanOrEqual, 5.1f, "t", null, true),

                GetBinary(BinaryTokenType.GreaterThanOrEqual, 5, 10, 5 >= 10, false),
                GetBinary(BinaryTokenType.GreaterThanOrEqual, 5.1f, 10, 5.1f >= 10, false),
                GetBinary(BinaryTokenType.GreaterThanOrEqual, 5.1f, "t", null, true),

                GetBinary(BinaryTokenType.Equality, 10, 10, 10 == 10, false),
                GetBinary(BinaryTokenType.Equality, 5.1f, 10, 5.1f == 10, false),
                GetBinary(BinaryTokenType.Equality, 5.1f, "t", false, false),

                GetBinary(BinaryTokenType.NotEqual, 10, 10, 10 != 10, false),
                GetBinary(BinaryTokenType.NotEqual, 5.1f, 10, 5.1f != 10, false),
                GetBinary(BinaryTokenType.NotEqual, 5.1f, "t", null, true),

                GetBinary(BinaryTokenType.LogicalAnd, 10, 10, 10 & 10, false),
                GetBinary(BinaryTokenType.LogicalAnd, 5.1f, 10, null, true),
                GetBinary(BinaryTokenType.LogicalAnd, 5.1f, "t", null, true),

                GetBinary(BinaryTokenType.LogicalXor, 10, 10, 10 ^ 10, false),
                GetBinary(BinaryTokenType.LogicalXor, 5.1f, 10, null, true),
                GetBinary(BinaryTokenType.LogicalXor, 5.1f, "t", null, true),

                GetBinary(BinaryTokenType.LogicalOr, 10, 10, 10 | 10, false),
                GetBinary(BinaryTokenType.LogicalOr, 5.1f, 10, null, true),
                GetBinary(BinaryTokenType.LogicalOr, 5.1f, "t", null, true),

                GetBinary(BinaryTokenType.ConditionalAnd, true, false, true && false, false),
                GetBinary(BinaryTokenType.ConditionalAnd, true, true, true && true, false),
                GetBinary(BinaryTokenType.ConditionalAnd, true, "t", null, true),

                GetBinary(BinaryTokenType.ConditionalOr, true, false, true || false, false),
                GetBinary(BinaryTokenType.ConditionalOr, true, true, true || true, false),
                GetBinary(BinaryTokenType.ConditionalOr, true, "t", null, true),

                GetBinary(BinaryTokenType.NullCoalescing, null, "f", null ?? "f", false),
                GetBinary(BinaryTokenType.NullCoalescing, "f", "t", "f" ?? "t", false),
                GetBinary(BinaryTokenType.NullCoalescing, "f", 1, null, true)
            };

        private static object?[] GetBinary(BinaryTokenType binaryToken, object? left, object? right, object? result, bool invalid)
        {
            var leftExpression = ConstantExpressionNode.Get(left);
            var rightExpression = ConstantExpressionNode.Get(right);
            return new[]
            {
                new BinaryExpressionNode(binaryToken, leftExpression, rightExpression),
                result,
                invalid
            };
        }
    }
}