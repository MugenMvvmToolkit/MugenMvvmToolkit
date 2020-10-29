using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Bindings.Parsing.Expressions;
using MugenMvvm.UnitTests.Bindings.Parsing.Internal;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Parsing.Expressions
{
    public class ExpressionNodeBaseTest : UnitTestBase
    {
        #region Methods

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void AcceptShouldRevisitNewExpression(bool isPostOrder)
        {
            const string member1 = "m1";
            const string member2 = "m2";
            const string member3 = "m3";

            var visitor = new TestExpressionVisitor
            {
                IsPostOrder = isPostOrder,
                Visit = (expressionNode, context) =>
                {
                    var memberExpressionNode = (IMemberExpressionNode) expressionNode;
                    if (memberExpressionNode.Member == member1)
                        return new MemberExpressionNode(null, member2);
                    if (memberExpressionNode.Member == member2)
                        return new MemberExpressionNode(null, member3);
                    return expressionNode;
                }
            };

            var node = new MemberExpressionNode(null, member1).Accept(visitor);
            node.ShouldEqual(new MemberExpressionNode(null, member3));
        }

        #endregion
    }
}