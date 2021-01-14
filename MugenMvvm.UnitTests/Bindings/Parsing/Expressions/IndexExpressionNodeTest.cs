using System;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Bindings.Parsing.Expressions;
using MugenMvvm.UnitTests.Bindings.Parsing.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Parsing.Expressions
{
    public class IndexExpressionNodeTest : UnitTestBase
    {
        [Fact]
        public void AcceptShouldCreateNewNode2()
        {
            var target = new ConstantExpressionNode("1");
            var args = new IExpressionNode[] {new ConstantExpressionNode("2")};
            var testExpressionVisitor = new TestExpressionVisitor
            {
                Visit = (node, context) => target
            };
            new IndexExpressionNode(target, args).Accept(testExpressionVisitor, DefaultMetadata).ShouldEqual(target);
        }

        [Fact]
        public void ConstructorShouldInitializeValues()
        {
            var target = new ConstantExpressionNode("1");
            var args = new IExpressionNode[] {new ConstantExpressionNode("2")};
            var exp = new IndexExpressionNode(target, args);
            exp.ExpressionType.ShouldEqual(ExpressionNodeType.Index);
            exp.Target.ShouldEqual(target);
            exp.Arguments.ShouldEqual(args);
            exp.ToString().ShouldEqual("\"1\"[\"2\"]");
        }

        [Fact]
        public void UpdateArgumentsShouldCreateNewNode()
        {
            var target = new ConstantExpressionNode("1");
            var args = new IExpressionNode[] {new ConstantExpressionNode("2")};
            var newArgs = new IExpressionNode[] {new ConstantExpressionNode("3")};
            var exp = new IndexExpressionNode(target, args);
            exp.UpdateArguments(args).ShouldEqual(exp);

            var newExp = exp.UpdateArguments(newArgs);
            newExp.ShouldNotEqual(exp);
            newExp.ExpressionType.ShouldEqual(ExpressionNodeType.Index);
            newExp.Target.ShouldEqual(target);
            newExp.Arguments.ShouldEqual(newArgs);
        }

        [Fact]
        public void UpdateTargetShouldCreateNewNode()
        {
            var target = new ConstantExpressionNode("1");
            var args = new IExpressionNode[] {new ConstantExpressionNode("2")};
            var newTarget = new ConstantExpressionNode("2");
            var exp = new IndexExpressionNode(target, args);
            exp.UpdateTarget(target).ShouldEqual(exp);

            var newExp = exp.UpdateTarget(newTarget);
            newExp.ShouldNotEqual(exp);
            newExp.ExpressionType.ShouldEqual(ExpressionNodeType.Index);
            newExp.Target.ShouldEqual(newTarget);
            newExp.Arguments.ShouldEqual(args);
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

            var target = new ConstantExpressionNode("1");
            var arg1 = new ConstantExpressionNode("2");
            var arg2 = new ConstantExpressionNode("3");
            var exp = new IndexExpressionNode(target, new[] {arg1, arg2});

            var result = visitor.TraversalType == ExpressionTraversalType.Preorder
                ? new IExpressionNode[] {exp, target, arg1, arg2}
                : new IExpressionNode[] {target, arg1, arg2, exp};
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
            var arg1 = new ConstantExpressionNode("2");
            var arg2 = new ConstantExpressionNode("3");

            var targetChanged = new ConstantExpressionNode("1-");
            var arg1Changed = new ConstantExpressionNode("2-");
            var arg2Changed = new ConstantExpressionNode("3-");
            var visitor = new TestExpressionVisitor
            {
                Visit = (node, context) =>
                {
                    if (node == target)
                        return targetChanged;
                    if (node == arg1)
                        return arg1Changed;
                    if (node == arg2)
                        return arg2Changed;
                    return node;
                },
                TraversalType = ExpressionTraversalType.Get(value)
            };
            var exp = new IndexExpressionNode(target, new[] {arg1, arg2});
            var expressionNode = (IndexExpressionNode) exp.Accept(visitor, DefaultMetadata);
            expressionNode.ShouldNotEqual(exp);
            expressionNode.Target.ShouldEqual(targetChanged);
            expressionNode.Arguments.AsList().ShouldEqual(new[] {arg1Changed, arg2Changed});
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void UpdateMetadataShouldCheckMetadataEquality(bool equal)
        {
            var target = new ConstantExpressionNode("1");
            var args = new IExpressionNode[] {new ConstantExpressionNode("2")};
            var node = new IndexExpressionNode(target, args, EmptyDictionary);
            if (equal)
                node.UpdateMetadata(EmptyDictionary).ShouldEqual(node, ReferenceEqualityComparer.Instance);
            else
            {
                var metadata = new Dictionary<string, object?> {{"k", null}};
                var updated = (IndexExpressionNode) node.UpdateMetadata(metadata);
                updated.ShouldNotEqual(node, ReferenceEqualityComparer.Instance);
                updated.Metadata.ShouldEqual(metadata);
                updated.Target.ShouldEqual(node.Target);
                updated.Arguments.ShouldEqual(node.Arguments);
            }
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        public void GetHashCodeEqualsShouldBeValid(bool withComparer, bool hasTarget)
        {
            var comparer = withComparer ? new TestExpressionEqualityComparer() : null;
            var exp1 = new IndexExpressionNode(hasTarget ? GetTestEqualityExpression(comparer, 1) : null,
                new IExpressionNode[] {GetTestEqualityExpression(comparer, 2), GetTestEqualityExpression(comparer, 3)},
                new Dictionary<string, object?> {{"k", null}});
            var exp2 = new IndexExpressionNode(hasTarget ? GetTestEqualityExpression(comparer, 1) : null,
                new IExpressionNode[] {GetTestEqualityExpression(comparer, 2), GetTestEqualityExpression(comparer, 3)},
                new Dictionary<string, object?> {{"k", null}});
            ;
            if (hasTarget)
            {
                HashCode.Combine(GetBaseHashCode(exp1), exp1.Arguments.Count, 1).ShouldEqual(exp1.GetHashCode(comparer));
                ((TestExpressionNode) exp1.Target!).GetHashCodeCount.ShouldEqual(1);
            }
            else
                HashCode.Combine(GetBaseHashCode(exp1), exp1.Arguments.Count).ShouldEqual(exp1.GetHashCode(comparer));

            exp1.Arguments.AsList().Cast<TestExpressionNode>().All(node => node.GetHashCodeCount == 0).ShouldBeTrue();

            exp1.Equals(exp2, comparer).ShouldBeTrue();
            ((TestExpressionNode?) exp1.Target)?.EqualsCount.ShouldEqual(1);
            exp1.Arguments.AsList().Cast<TestExpressionNode>().All(node => node.EqualsCount == 1).ShouldBeTrue();

            exp1.Equals(exp2.UpdateMetadata(null), comparer).ShouldBeFalse();
            ((TestExpressionNode?) exp1.Target)?.EqualsCount.ShouldEqual(1);
            exp1.Arguments.AsList().Cast<TestExpressionNode>().All(node => node.EqualsCount == 1).ShouldBeTrue();

            if (comparer == null || !hasTarget)
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
            ((TestExpressionNode) exp1.Target!).EqualsCount.ShouldEqual(1);
            exp1.Arguments.AsList().Cast<TestExpressionNode>().All(node => node.EqualsCount == 1).ShouldBeTrue();
        }
    }
}