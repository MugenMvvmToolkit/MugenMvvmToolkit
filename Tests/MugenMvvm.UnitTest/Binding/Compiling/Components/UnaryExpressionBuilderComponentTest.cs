using System.Collections.Generic;
using MugenMvvm.Binding.Compiling.Components;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Compiling;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Binding.Parsing.Expressions;
using MugenMvvm.UnitTest.Binding.Compiling.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Compiling.Components
{
    public class UnaryExpressionBuilderComponentTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void TryBuildShouldIgnoreNotUnaryExpression()
        {
            var component = new UnaryExpressionBuilderComponent();
            var ctx = new TestExpressionBuilderContext();
            component.TryBuild(ctx, ConstantExpressionNode.False).ShouldBeNull();
        }

        [Fact]
        public void TryBuildShouldIgnoreNotSupportUnaryExpression()
        {
            var component = new UnaryExpressionBuilderComponent();
            component.UnaryTokenMapping.Clear();
            var ctx = new TestExpressionBuilderContext();
            component.TryBuild(ctx, new UnaryExpressionNode(UnaryTokenType.LogicalNegation, ConstantExpressionNode.False)).ShouldBeNull();
        }

        [Theory]
        [MemberData(nameof(GetData))]
        public void TryBuildShouldBuildUnaryExpression(IUnaryExpressionNode unaryExpression, IExpressionBuilderContext context, object result, bool invalid)
        {
            var component = new UnaryExpressionBuilderComponent();
            if (invalid)
            {
                ShouldThrow(() => component.TryBuild(context, unaryExpression));
                return;
            }

            var expression = component.TryBuild(context, unaryExpression)!;
            expression.ShouldNotBeNull();
            expression.Invoke().ShouldEqual(result);
        }

        public static IEnumerable<object?[]> GetData()
        {
            return new[]
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
        }

        private static object?[] GetUnary(UnaryTokenType unaryTokenType, object? operand, object? result, bool invalid)
        {
            var expression = ConstantExpressionNode.Get(operand);
            return new[]
            {
                new UnaryExpressionNode(unaryTokenType, expression),
                new TestExpressionBuilderContext(),
                result,
                invalid
            };
        }

        #endregion
    }
}