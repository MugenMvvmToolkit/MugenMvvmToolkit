using System;
using MugenMvvm.Bindings.Compiling.Components;
using MugenMvvm.Bindings.Parsing.Expressions;
using MugenMvvm.UnitTests.Bindings.Compiling.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Compiling.Components
{
    public class ConditionExpressionBuilderTest : UnitTestBase
    {
        [Fact]
        public void TryBuildShouldIgnoreNotConditionExpression()
        {
            var component = new ConditionExpressionBuilder();
            var ctx = new TestExpressionBuilderContext();
            component.TryBuild(ctx, ConstantExpressionNode.False).ShouldBeNull();
        }

        [Fact]
        public void TryBuildShouldThrowInvalidException()
        {
            var component = new ConditionExpressionBuilder();
            var conditionExpression = ConstantExpressionNode.Get(1);
            var ifTrueExp = ConstantExpressionNode.Get(0);
            var ifFalseExp = ConstantExpressionNode.Get(1);
            var context = new TestExpressionBuilderContext();
            ShouldThrow<InvalidOperationException>(() => component.TryBuild(context, new ConditionExpressionNode(conditionExpression, ifTrueExp, ifFalseExp)));
        }

        [Theory]
        [InlineData(true, 1, 2, 1)]
        [InlineData(false, 1, 2, 2)]
        public void TryBuildShouldBuildConditionExpression(bool condition, object ifTrueValue, object ifFalseValue, object result)
        {
            var component = new ConditionExpressionBuilder();
            var conditionExpression = ConstantExpressionNode.Get(condition);
            var ifTrueExp = ConstantExpressionNode.Get(ifTrueValue);
            var ifFalseExp = ConstantExpressionNode.Get(ifFalseValue);
            var context = new TestExpressionBuilderContext();
            component.TryBuild(context, new ConditionExpressionNode(conditionExpression, ifTrueExp, ifFalseExp))!.Invoke().ShouldEqual(result);
        }
    }
}