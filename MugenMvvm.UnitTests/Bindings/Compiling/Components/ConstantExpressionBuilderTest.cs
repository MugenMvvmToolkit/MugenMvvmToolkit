using MugenMvvm.Bindings.Compiling.Components;
using MugenMvvm.Bindings.Parsing.Expressions;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Compiling.Components
{
    public class ConstantExpressionBuilderTest : ExpressionBuilderTestBase<ConstantExpressionBuilder>
    {
        [Fact]
        public void TryBuildShouldBuildConstantExpression()
        {
            var build = Builder.TryBuild(Context, ConstantExpressionNode.False)!;
            build.ShouldEqual(ConstantExpressionNode.False.ConstantExpression);
            build.Invoke().ShouldEqual(false);

            var constantExpressionNode = new ConstantExpressionNode(1);
            build = Builder.TryBuild(Context, constantExpressionNode)!;
            build.Invoke().ShouldEqual(constantExpressionNode.Value);
        }

        [Fact]
        public void TryBuildShouldIgnoreNotConstantExpression() => Builder.TryBuild(Context, MemberExpressionNode.Self).ShouldBeNull();
    }
}