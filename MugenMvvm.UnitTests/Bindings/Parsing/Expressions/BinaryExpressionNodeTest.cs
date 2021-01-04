using System;
using System.Collections.Generic;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Bindings.Parsing.Expressions;
using MugenMvvm.UnitTests.Bindings.Parsing.Internal;
using MugenMvvm.UnitTests.Internal.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Parsing.Expressions
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
            exp.ToString().ShouldEqual("(\"1\" == \"2\")");
        }

        [Theory]
        [InlineData(ExpressionTraversalType.InorderValue)]
        [InlineData(ExpressionTraversalType.PreorderValue)]
        [InlineData(ExpressionTraversalType.PostorderValue)]
        public void AcceptShouldVisitWithCorrectOrder(int value)
        {
            var nodes = new List<IExpressionNode>();
            var visitor = new TestExpressionVisitor
            {
                Visit = (node, context) =>
                {
                    nodes.Add(node);
                    context.ShouldEqual(DefaultMetadata);
                    return node;
                },
                TraversalType = ExpressionTraversalType.Get(value)
            };

            var exp5 = new ConstantExpressionNode("5");
            var exp4 = new ConstantExpressionNode("4");
            var exp3 = new ConstantExpressionNode("3");
            var exp2 = new BinaryExpressionNode(BinaryTokenType.Equality, exp4, exp5);
            var exp1 = new BinaryExpressionNode(BinaryTokenType.Addition, exp2, exp3);

            exp1.Accept(visitor, DefaultMetadata).ShouldEqual(exp1);
            if (visitor.TraversalType == ExpressionTraversalType.Inorder)
                nodes.ShouldEqual(new IExpressionNode[] {exp4, exp2, exp5, exp1, exp3});
            else if (visitor.TraversalType == ExpressionTraversalType.Preorder)
                nodes.ShouldEqual(new IExpressionNode[] {exp1, exp2, exp4, exp5, exp3});
            else if (visitor.TraversalType == ExpressionTraversalType.Postorder)
                nodes.ShouldEqual(new IExpressionNode[] {exp4, exp5, exp2, exp3, exp1});
        }

        [Theory]
        [InlineData(ExpressionTraversalType.InorderValue)]
        [InlineData(ExpressionTraversalType.PreorderValue)]
        [InlineData(ExpressionTraversalType.PostorderValue)]
        public void AcceptShouldCreateNewNode1(int value)
        {
            var left = new ConstantExpressionNode("1");
            var right = new ConstantExpressionNode("2");
            var leftChanged = new ConstantExpressionNode("-");
            var rightChanged = new ConstantExpressionNode("--");
            var visitor = new TestExpressionVisitor
            {
                Visit = (node, context) =>
                {
                    if (node == left)
                        return leftChanged;
                    if (node == right)
                        return rightChanged;
                    return node;
                },
                TraversalType = ExpressionTraversalType.Get(value)
            };
            var exp = new BinaryExpressionNode(BinaryTokenType.Equality, left, right);
            var expressionNode = (BinaryExpressionNode) exp.Accept(visitor, DefaultMetadata);
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

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void UpdateMetadataShouldCheckMetadataEquality(bool equal)
        {
            var left = new ConstantExpressionNode("1");
            var right = new ConstantExpressionNode("2");
            var exp = new BinaryExpressionNode(BinaryTokenType.Addition, left, right, EmptyDictionary);
            if (equal)
                exp.UpdateMetadata(EmptyDictionary).ShouldEqual(exp, ReferenceEqualityComparer.Instance);
            else
            {
                var metadata = new Dictionary<string, object?> {{"k", null}};
                var updated = (BinaryExpressionNode) exp.UpdateMetadata(metadata);
                updated.ShouldNotEqual(exp, ReferenceEqualityComparer.Instance);
                updated.Metadata.ShouldEqual(metadata);
                updated.Token.ShouldEqual(exp.Token);
                updated.Left.ShouldEqual(exp.Left);
                updated.Right.ShouldEqual(exp.Right);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void GetHashCodeEqualsShouldBeValid(bool withComparer)
        {
            var comparer = withComparer ? new TestExpressionEqualityComparer() : null;
            var exp1 = new BinaryExpressionNode(BinaryTokenType.Addition, GetTestEqualityExpression(comparer, 0), GetTestEqualityExpression(comparer, 1), new Dictionary<string, object?> {{"k", null}});
            var exp2 = new BinaryExpressionNode(BinaryTokenType.Addition, GetTestEqualityExpression(comparer, 0), GetTestEqualityExpression(comparer, 1), new Dictionary<string, object?> {{"k", null}});
            HashCode.Combine(GetBaseHashCode(exp1), exp1.Token.GetHashCode(), 0, 1).ShouldEqual(exp1.GetHashCode(comparer));
            ((TestExpressionNode) exp1.Left).GetHashCodeCount.ShouldEqual(1);
            ((TestExpressionNode) exp1.Right).GetHashCodeCount.ShouldEqual(1);

            exp1.Equals(exp2, comparer).ShouldBeTrue();
            ((TestExpressionNode) exp1.Left).EqualsCount.ShouldEqual(1);
            ((TestExpressionNode) exp1.Right).EqualsCount.ShouldEqual(1);

            exp1.Equals(exp2.UpdateMetadata(null), comparer).ShouldBeFalse();
            ((TestExpressionNode) exp1.Left).EqualsCount.ShouldEqual(1);
            ((TestExpressionNode) exp1.Right).EqualsCount.ShouldEqual(1);

            if (comparer == null)
                return;
            comparer.GetHashCode = node =>
            {
                ReferenceEquals(node, exp1).ShouldBeTrue();
                return int.MaxValue;
            };
            comparer.Equals = (x1, x2) =>
            {
                ReferenceEquals(x1, exp1).ShouldBeTrue();
                ReferenceEquals(x2, exp2).ShouldBeTrue();
                return false;
            };
            exp1.GetHashCode(comparer).ShouldEqual(int.MaxValue);
            exp1.Equals(exp2, comparer).ShouldBeFalse();
            ((TestExpressionNode) exp1.Left).EqualsCount.ShouldEqual(1);
            ((TestExpressionNode) exp1.Right).EqualsCount.ShouldEqual(1);
        }

        #endregion
    }
}