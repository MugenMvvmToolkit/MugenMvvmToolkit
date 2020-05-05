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
            var binaryExpressionNode = new BinaryExpressionNode(tokenType, left, right);
            binaryExpressionNode.ExpressionType.ShouldEqual(ExpressionNodeType.Binary);
            binaryExpressionNode.Left.ShouldEqual(left);
            binaryExpressionNode.Right.ShouldEqual(right);
            binaryExpressionNode.Token.ShouldEqual(tokenType);
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
            var binaryExpressionNode = new BinaryExpressionNode(BinaryTokenType.Equality, left, right);

            var result = isPostOrder ? new IExpressionNode[] {left, right, binaryExpressionNode} : new IExpressionNode[] {binaryExpressionNode, left, right};
            binaryExpressionNode.Accept(testExpressionVisitor, DefaultMetadata).ShouldEqual(binaryExpressionNode);
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
            var binaryExpressionNode = new BinaryExpressionNode(BinaryTokenType.Equality, left, right);
            var expressionNode = (BinaryExpressionNode) binaryExpressionNode.Accept(testExpressionVisitor, DefaultMetadata);
            expressionNode.ShouldNotEqual(binaryExpressionNode);
            expressionNode.Left.ShouldEqual(leftChanged);
            expressionNode.Right.ShouldEqual(rightChanged);
            expressionNode.ExpressionType.ShouldEqual(binaryExpressionNode.ExpressionType);
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
            var binaryExpressionNode = new BinaryExpressionNode(BinaryTokenType.Equality, left, right);
            binaryExpressionNode.Accept(testExpressionVisitor, DefaultMetadata).ShouldEqual(left);
        }

        #endregion
    }
}