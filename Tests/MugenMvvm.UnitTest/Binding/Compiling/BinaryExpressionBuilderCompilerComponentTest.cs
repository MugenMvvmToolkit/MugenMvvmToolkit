using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using MugenMvvm.Binding.Compiling.Components;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Compiling;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Binding.Parsing.Expressions;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Compiling
{
    public class BinaryExpressionBuilderCompilerComponentTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void TryBuildShouldNotSupportNonBinaryExpression()
        {
            var component = new BinaryExpressionBuilderCompilerComponent();
            var ctx = new TestExpressionBuilderContext();
            component.TryBuild(ctx, ConstantExpressionNode.False).ShouldBeNull();
        }

        [Fact]
        public void TryBuildShouldIgnoreNotSupportBinaryExpression()
        {
            var component = new BinaryExpressionBuilderCompilerComponent();
            component.BinaryTokenMapping.Clear();
            var ctx = new TestExpressionBuilderContext();
            component.TryBuild(ctx, new BinaryExpressionNode(BinaryTokenType.LogicalOr, ConstantExpressionNode.False, ConstantExpressionNode.False)).ShouldBeNull();
        }

        [Theory]
        [MemberData(nameof(GetData))]
        public void TryBuildShouldBuildBinaryExpression(IBinaryExpressionNode binaryExpression, IExpressionBuilderContext context, object result, bool invalid)
        {
            var component = new BinaryExpressionBuilderCompilerComponent();
            if (invalid)
            {
                ShouldThrow(() => component.TryBuild(context, binaryExpression));
                return;
            }

            var expression = component.TryBuild(context, binaryExpression);
            expression.ShouldNotBeNull();
            Expression.Lambda(expression).Compile().DynamicInvoke().ShouldEqual(result);
        }

        public static IEnumerable<object[]> GetData()
        {
            return new[]
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
                GetBinary(BinaryTokenType.NullCoalescing, "f", 1, null, true),
            };
        }

        private static object[] GetBinary(BinaryTokenType binaryToken, object left, object right, object result, bool invalid)
        {
            var leftExpression = ConstantExpressionNode.Get(left);
            var rightExpression = ConstantExpressionNode.Get(right);
            var context = new TestExpressionBuilderContext
            {
                Build = node =>
                {
                    if (node == leftExpression)
                        return Expression.Constant(left);
                    if (node == rightExpression)
                        return Expression.Constant(right);
                    throw new NotSupportedException();
                }
            };
            return new[]
            {
                new BinaryExpressionNode(binaryToken, leftExpression, rightExpression),
                context,
                result,
                invalid
            };
        }

        #endregion
    }
}