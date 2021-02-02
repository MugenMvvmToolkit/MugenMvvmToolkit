using System.Collections.Generic;
using MugenMvvm.Bindings.Compiling.Components;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Bindings.Parsing.Expressions;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Compiling.Components
{
    public class UnaryExpressionBuilderTest : ExpressionBuilderTestBase<UnaryExpressionBuilder>
    {
        [Fact]
        public void TryBuildShouldIgnoreNotSupportUnaryExpression()
        {
            Builder.Mapping.Clear();
            Builder.TryBuild(Context, new UnaryExpressionNode(UnaryTokenType.LogicalNegation, ConstantExpressionNode.False)).ShouldBeNull();
        }

        [Fact]
        public void TryBuildShouldIgnoreNotUnaryExpression() => Builder.TryBuild(Context, ConstantExpressionNode.False).ShouldBeNull();

        [Theory]
        [MemberData(nameof(GetData))]
        public void TryBuildShouldBuildUnaryExpression(IUnaryExpressionNode unaryExpression, object result, bool invalid)
        {
            if (invalid)
            {
                ShouldThrow(() => Builder.TryBuild(Context, unaryExpression));
                return;
            }

            var expression = Builder.TryBuild(Context, unaryExpression)!;
            expression.ShouldNotBeNull();
            expression.Invoke().ShouldEqual(result);
        }

        public static IEnumerable<object?[]> GetData() =>
            new[]
            {
                GetUnary(UnaryTokenType.Minus, -1, 1, false),
                GetUnary(UnaryTokenType.Minus, 1, -1, false),
                GetUnary(UnaryTokenType.Minus, "", null, true),

                GetUnary(UnaryTokenType.Plus, +-1, -1, false),
                GetUnary(UnaryTokenType.Plus, +1, 1, false),
                GetUnary(UnaryTokenType.Plus, "", null, true),

                GetUnary(UnaryTokenType.LogicalNegation, true, !true, false),
                GetUnary(UnaryTokenType.LogicalNegation, false, !false, false),
                GetUnary(UnaryTokenType.LogicalNegation, "", null, true),

                GetUnary(UnaryTokenType.BitwiseNegation, -1, ~-1, false),
                GetUnary(UnaryTokenType.BitwiseNegation, 1, ~1, false),
                GetUnary(UnaryTokenType.BitwiseNegation, "", null, true)
            };

        private static object?[] GetUnary(UnaryTokenType unaryTokenType, object? operand, object? result, bool invalid)
        {
            var expression = ConstantExpressionNode.Get(operand);
            return new[]
            {
                new UnaryExpressionNode(unaryTokenType, expression),
                result,
                invalid
            };
        }
    }
}