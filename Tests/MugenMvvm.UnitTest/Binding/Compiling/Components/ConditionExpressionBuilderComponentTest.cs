using System;
using MugenMvvm.Binding.Compiling.Components;
using MugenMvvm.Binding.Parsing.Expressions;
using MugenMvvm.UnitTest.Binding.Compiling.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Compiling.Components
{
    public class ConditionExpressionBuilderComponentTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void TryBuildShouldIgnoreNotConditionExpression()
        {
            var component = new ConditionExpressionBuilderComponent();
            var ctx = new TestExpressionBuilderContext();
            component.TryBuild(ctx, ConstantExpressionNode.False).ShouldBeNull();
        }

        [Theory]
        [InlineData(true, 1, 2, 1)]
        [InlineData(false, 1, 2, 2)]
        public void TryBuildShouldBuildConditionExpression(bool condition, object ifTrueValue, object ifFalseValue, object result)
        {
            var component = new ConditionExpressionBuilderComponent();
            var conditionExpression = ConstantExpressionNode.Get(condition);
            var ifTrueExp = ConstantExpressionNode.Get(ifTrueValue);
            var ifFalseExp = ConstantExpressionNode.Get(ifFalseValue);
            var context = new TestExpressionBuilderContext();
            component.TryBuild(context, new ConditionExpressionNode(conditionExpression, ifTrueExp, ifFalseExp))!.Invoke().ShouldEqual(result);
        }

        [Fact]
        public void TryBuildShouldThrowInvalidException()
        {
            var component = new ConditionExpressionBuilderComponent();
            var conditionExpression = ConstantExpressionNode.Get(1);
            var ifTrueExp = ConstantExpressionNode.Get(0);
            var ifFalseExp = ConstantExpressionNode.Get(1);
            var context = new TestExpressionBuilderContext();
            ShouldThrow<InvalidOperationException>(() => component.TryBuild(context, new ConditionExpressionNode(conditionExpression, ifTrueExp, ifFalseExp)));
        }

        #endregion
    }
}