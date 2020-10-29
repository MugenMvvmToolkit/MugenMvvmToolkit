using MugenMvvm.Bindings.Compiling.Components;
using MugenMvvm.Bindings.Parsing.Expressions;
using MugenMvvm.UnitTests.Bindings.Compiling.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Compiling.Components
{
    public class ConstantExpressionBuilderTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void TryBuildShouldIgnoreNotConstantExpression()
        {
            var component = new ConstantExpressionBuilder();
            var ctx = new TestExpressionBuilderContext();
            component.TryBuild(ctx, MemberExpressionNode.Self).ShouldBeNull();
        }

        [Fact]
        public void TryBuildShouldBuildConstantExpression()
        {
            var component = new ConstantExpressionBuilder();
            var ctx = new TestExpressionBuilderContext();
            var build = component.TryBuild(ctx, ConstantExpressionNode.False)!;
            build.ShouldEqual(ConstantExpressionNode.False.ConstantExpression);
            build.Invoke().ShouldEqual(false);

            var constantExpressionNode = new ConstantExpressionNode(1);
            build = component.TryBuild(ctx, constantExpressionNode)!;
            build.Invoke().ShouldEqual(constantExpressionNode.Value);
        }

        #endregion
    }
}