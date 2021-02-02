using System;
using MugenMvvm.Bindings.Compiling.Components;
using MugenMvvm.Bindings.Parsing.Expressions;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Compiling.Components
{
    public class ConditionExpressionBuilderTest : ExpressionBuilderTestBase<ConditionExpressionBuilder>
    {
        [Fact]
        public void TryBuildShouldIgnoreNotConditionExpression() => Builder.TryBuild(Context, ConstantExpressionNode.False).ShouldBeNull();

        [Fact]
        public void TryBuildShouldThrowInvalidException()
        {
            var conditionExpression = ConstantExpressionNode.Get(1);
            var ifTrueExp = ConstantExpressionNode.Get(0);
            var ifFalseExp = ConstantExpressionNode.Get(1);
            ShouldThrow<InvalidOperationException>(() => Builder.TryBuild(Context, new ConditionExpressionNode(conditionExpression, ifTrueExp, ifFalseExp)));
        }

        [Theory]
        [InlineData(true, 1, 2, 1)]
        [InlineData(false, 1, 2, 2)]
        public void TryBuildShouldBuildConditionExpression(bool condition, object ifTrueValue, object ifFalseValue, object result)
        {
            var conditionExpression = ConstantExpressionNode.Get(condition);
            var ifTrueExp = ConstantExpressionNode.Get(ifTrueValue);
            var ifFalseExp = ConstantExpressionNode.Get(ifFalseValue);
            Builder.TryBuild(Context, new ConditionExpressionNode(conditionExpression, ifTrueExp, ifFalseExp))!.Invoke().ShouldEqual(result);
        }
    }
}