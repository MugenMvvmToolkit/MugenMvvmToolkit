using System;
using System.Collections.Generic;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Bindings.Parsing.Expressions;
using MugenMvvm.UnitTests.Bindings.Parsing.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Parsing.Expressions
{
    public class NullConditionalMemberExpressionNodeTest : UnitTestBase
    {
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

            var target = new ConstantExpressionNode("1");
            var exp = new NullConditionalMemberExpressionNode(target);

            var result = visitor.TraversalType == ExpressionTraversalType.Preorder ? new IExpressionNode[] {exp, target} : new IExpressionNode[] {target, exp};
            exp.Accept(visitor, DefaultMetadata).ShouldEqual(exp);
            result.ShouldEqual(nodes);
        }

        [Theory]
        [InlineData(ExpressionTraversalType.InorderValue)]
        [InlineData(ExpressionTraversalType.PreorderValue)]
        [InlineData(ExpressionTraversalType.PostorderValue)]
        public void AcceptShouldCreateNewNode1(int value)
        {
            var target = new ConstantExpressionNode("1");
            var targetChanged = new ConstantExpressionNode("1-");
            var visitor = new TestExpressionVisitor
            {
                Visit = (node, context) =>
                {
                    if (node == target)
                        return targetChanged;
                    return node;
                },
                TraversalType = ExpressionTraversalType.Get(value)
            };
            var exp = new NullConditionalMemberExpressionNode(target);
            var expressionNode = (NullConditionalMemberExpressionNode) exp.Accept(visitor, DefaultMetadata);
            expressionNode.ShouldNotEqual(exp);
            expressionNode.Target.ShouldEqual(targetChanged);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void UpdateMetadataShouldCheckMetadataEquality(bool equal)
        {
            var target = new ConstantExpressionNode("1");
            var node = new NullConditionalMemberExpressionNode(target, EmptyDictionary);
            if (equal)
                node.UpdateMetadata(EmptyDictionary).ShouldEqual(node, ReferenceEqualityComparer.Instance);
            else
            {
                var metadata = new Dictionary<string, object?> {{"k", null}};
                var updated = (NullConditionalMemberExpressionNode) node.UpdateMetadata(metadata);
                updated.ShouldNotEqual(node, ReferenceEqualityComparer.Instance);
                updated.Metadata.ShouldEqual(metadata);
                updated.Target.ShouldEqual(node.Target);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void GetHashCodeEqualsShouldBeValid(bool withComparer)
        {
            var comparer = withComparer ? new TestExpressionEqualityComparer() : null;
            var exp1 = new NullConditionalMemberExpressionNode(GetTestEqualityExpression(comparer, 1), new Dictionary<string, object?> {{"k", null}});
            var exp2 = new NullConditionalMemberExpressionNode(GetTestEqualityExpression(comparer, 1), new Dictionary<string, object?> {{"k", null}});
            HashCode.Combine(GetBaseHashCode(exp1), 1).ShouldEqual(exp1.GetHashCode(comparer));
            ((TestExpressionNode) exp1.Target).GetHashCodeCount.ShouldEqual(1);

            exp1.Equals(exp2, comparer).ShouldBeTrue();
            ((TestExpressionNode) exp1.Target).EqualsCount.ShouldEqual(1);

            exp1.Equals(exp2.UpdateMetadata(null), comparer).ShouldBeFalse();
            ((TestExpressionNode) exp1.Target).EqualsCount.ShouldEqual(1);

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
            ((TestExpressionNode) exp1.Target).EqualsCount.ShouldEqual(1);
        }

        [Fact]
        public void AcceptShouldCreateNewNode2()
        {
            var target = new ConstantExpressionNode("1");
            var testExpressionVisitor = new TestExpressionVisitor
            {
                Visit = (node, context) => target
            };
            new NullConditionalMemberExpressionNode(target).Accept(testExpressionVisitor, DefaultMetadata).ShouldEqual(target);
        }

        [Fact]
        public void ConstructorShouldInitializeValues()
        {
            var target = new ConstantExpressionNode("1");
            var exp = new NullConditionalMemberExpressionNode(target);
            exp.ExpressionType.ShouldEqual(ExpressionNodeType.Member);
            exp.Target.ShouldEqual(target);
            exp.ToString().ShouldEqual("\"1\"?");
        }

        [Fact]
        public void UpdateTargetShouldCreateNewNode()
        {
            var target = new ConstantExpressionNode("1");
            var newTarget = new ConstantExpressionNode("2");
            var exp = new NullConditionalMemberExpressionNode(target);
            exp.UpdateTarget(target).ShouldEqual(exp);

            var newExp = exp.UpdateTarget(newTarget);
            newExp.ShouldNotEqual(exp);
            newExp.ExpressionType.ShouldEqual(ExpressionNodeType.Member);
            newExp.Target.ShouldEqual(newTarget);
        }
    }
}