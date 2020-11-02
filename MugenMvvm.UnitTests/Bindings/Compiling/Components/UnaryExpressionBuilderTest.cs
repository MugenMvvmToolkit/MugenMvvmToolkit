﻿using System.Collections.Generic;
using MugenMvvm.Bindings.Compiling.Components;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Compiling;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Bindings.Parsing.Expressions;
using MugenMvvm.UnitTests.Bindings.Compiling.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Compiling.Components
{
    public class UnaryExpressionBuilderTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void TryBuildShouldIgnoreNotUnaryExpression()
        {
            var component = new UnaryExpressionBuilder();
            var ctx = new TestExpressionBuilderContext();
            component.TryBuild(ctx, ConstantExpressionNode.False).ShouldBeNull();
        }

        [Fact]
        public void TryBuildShouldIgnoreNotSupportUnaryExpression()
        {
            var component = new UnaryExpressionBuilder();
            component.Mapping.Clear();
            var ctx = new TestExpressionBuilderContext();
            component.TryBuild(ctx, new UnaryExpressionNode(UnaryTokenType.LogicalNegation, ConstantExpressionNode.False)).ShouldBeNull();
        }

        [Theory]
        [MemberData(nameof(GetData))]
        public void TryBuildShouldBuildUnaryExpression(IUnaryExpressionNode unaryExpression, IExpressionBuilderContext context, object result, bool invalid)
        {
            var component = new UnaryExpressionBuilder();
            if (invalid)
            {
                ShouldThrow(() => component.TryBuild(context, unaryExpression));
                return;
            }

            var expression = component.TryBuild(context, unaryExpression)!;
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
                new TestExpressionBuilderContext(),
                result,
                invalid
            };
        }

        #endregion
    }
}