using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Binding.Parsing.Expressions;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Parsing
{
    public class BinaryExpressionNodeTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void ConstructorShouldInitializeValues()
        {
            BinaryTokenType tokenType = BinaryTokenType.Equality;
            var left = new ConstantExpressionNode("1");
            var right = new ConstantExpressionNode("2");
            var exp = new BinaryExpressionNode(tokenType, left, right);
            exp.ExpressionType.ShouldEqual(ExpressionNodeType.Binary);
            exp.Left.ShouldEqual(left);
            exp.Right.ShouldEqual(right);
            exp.Token.ShouldEqual(tokenType);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void AcceptShouldVisitWithCorrectOrder(bool isPostOrder)
        {
            var nodes = new List<IExpressionNode>();
            var testExpressionVisitor = new TestExpressionVisitor
            {
                Visit = (node, context) =>
                {
                    nodes.Add(node);
                    context.ShouldEqual(DefaultMetadata);
                    return node;
                },
                IsPostOrder = isPostOrder
            };

            var left = new ConstantExpressionNode("1");
            var right = new ConstantExpressionNode("2");
            var exp = new BinaryExpressionNode(BinaryTokenType.Equality, left, right);

            var result = isPostOrder ? new IExpressionNode[] {left, right, exp} : new IExpressionNode[] {exp, left, right};
            exp.Accept(testExpressionVisitor, DefaultMetadata).ShouldEqual(exp);
            result.SequenceEqual(nodes).ShouldBeTrue();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void AcceptShouldCreateNewNode1(bool isPostOrder)
        {
            var left = new ConstantExpressionNode("1");
            var right = new ConstantExpressionNode("2");
            var leftChanged = new ConstantExpressionNode("-");
            var rightChanged = new ConstantExpressionNode("--");
            var testExpressionVisitor = new TestExpressionVisitor
            {
                Visit = (node, context) =>
                {
                    if (node == left)
                        return leftChanged;
                    if (node == right)
                        return rightChanged;
                    return node;
                },
                IsPostOrder = isPostOrder
            };
            var exp = new BinaryExpressionNode(BinaryTokenType.Equality, left, right);
            var expressionNode = (BinaryExpressionNode) exp.Accept(testExpressionVisitor, DefaultMetadata);
            expressionNode.ShouldNotEqual(exp);
            expressionNode.Left.ShouldEqual(leftChanged);
            expressionNode.Right.ShouldEqual(rightChanged);
            expressionNode.Token.ShouldEqual(exp.Token);
        }

        [Fact]
        public void AcceptShouldCreateNewNode2()
        {
            var left = new ConstantExpressionNode("1");
            var right = new ConstantExpressionNode("2");
            var testExpressionVisitor = new TestExpressionVisitor
            {
                Visit = (node, context) => left
            };
            new BinaryExpressionNode(BinaryTokenType.Equality, left, right).Accept(testExpressionVisitor, DefaultMetadata).ShouldEqual(left);
        }

        #endregion
    }
}