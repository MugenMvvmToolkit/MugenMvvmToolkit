using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Parsing.Expressions;
using MugenMvvm.Bindings.Parsing.Expressions.Binding;
using MugenMvvm.Bindings.Parsing.Visitors;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Parsing.Visitors
{
    public class ConstantToBindingParameterVisitorTest : UnitTestBase
    {
        [Fact]
        public void ShouldConvertConstantToBindingInstanceMember()
        {
            var visitor = new ConstantToBindingParameterVisitor();
            var expression = new ConstantExpressionNode("1");
            expression.Accept(visitor, Metadata)
                      .ShouldEqual(new BindingInstanceMemberExpressionNode(expression.Value, "", -1, default, MemberFlags.Static, null, expression, expression.Metadata));

            var memberExp = new MemberExpressionNode(expression, "M");
            memberExp.Accept(visitor, Metadata)
                     .ShouldEqual(memberExp.UpdateTarget(new BindingInstanceMemberExpressionNode(expression.Value, "", -1, default, MemberFlags.Static, null, expression,
                         expression.Metadata)));
        }
    }
}