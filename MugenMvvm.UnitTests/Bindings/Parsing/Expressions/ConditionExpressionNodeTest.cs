using System;
using System.Collections.Generic;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Bindings.Parsing.Expressions;
using MugenMvvm.Tests.Bindings.Parsing;
using MugenMvvm.UnitTests.Bindings.Parsing.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Parsing.Expressions
{
    public class ConditionExpressionNodeTest : UnitTestBase
    {
        [Fact]
        public void AcceptShouldCreateNewNode2()
        {
            var condition = new ConstantExpressionNode("-");
            var ifTrue = new ConstantExpressionNode("1");
            var ifFalse = new ConstantExpressionNode("2");
            var testExpressionVisitor = new TestExpressionVisitor
            {
                Visit = (node, context) => condition
            };
            new ConditionExpressionNode(condition, ifTrue, ifFalse).Accept(testExpressionVisitor, DefaultMetadata).ShouldEqual(condition);
        }

        [Fact]
        public void ConstructorShouldInitializeValues()
        {
            var condition = new ConstantExpressionNode("-");
            var ifTrue = new ConstantExpressionNode("1");
            var ifFalse = new ConstantExpressionNode("2");
            var exp = new ConditionExpressionNode(condition, ifTrue, ifFalse);
            exp.ExpressionType.ShouldEqual(ExpressionNodeType.Condition);
            exp.Condition.ShouldEqual(condition);
            exp.IfFalse.ShouldEqual(ifFalse);
            exp.IfTrue.ShouldEqual(ifTrue);
            exp.ToString().ShouldEqual("if (\"-\") {\"1\"} else {\"2\"}");
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

            var condition = new ConstantExpressionNode("-");
            var ifTrue = new ConstantExpressionNode("1");
            var ifFalse = new ConstantExpressionNode("2");
            var exp = new ConditionExpressionNode(condition, ifTrue, ifFalse);

            var result = visitor.TraversalType == ExpressionTraversalType.Preorder
                ? new IExpressionNode[] {exp, condition, ifTrue, ifFalse}
                : new IExpressionNode[] {condition, ifTrue, ifFalse, exp};
            exp.Accept(visitor, DefaultMetadata).ShouldEqual(exp);
            result.ShouldEqual(nodes);
        }

        [Theory]
        [InlineData(ExpressionTraversalType.InorderValue)]
        [InlineData(ExpressionTraversalType.PreorderValue)]
        [InlineData(ExpressionTraversalType.PostorderValue)]
        public void AcceptShouldCreateNewNode1(int value)
        {
            var condition = new ConstantExpressionNode("-");
            var ifTrue = new ConstantExpressionNode("1");
            var ifFalse = new ConstantExpressionNode("2");
            var conditionChanged = new ConstantExpressionNode("--");
            var ifTrueChanged = new ConstantExpressionNode("1-");
            var ifFalseChanged = new ConstantExpressionNode("2-");
            var visitor = new TestExpressionVisitor
            {
                Visit = (node, context) =>
                {
                    if (node == condition)
                        return conditionChanged;
                    if (node == ifTrue)
                        return ifTrueChanged;
                    if (node == ifFalse)
                        return ifFalseChanged;
                    return node;
                },
                TraversalType = ExpressionTraversalType.Get(value)
            };
            var exp = new ConditionExpressionNode(condition, ifTrue, ifFalse);
            var expressionNode = (ConditionExpressionNode) exp.Accept(visitor, DefaultMetadata);
            expressionNode.ShouldNotEqual(exp);
            expressionNode.Condition.ShouldEqual(conditionChanged);
            expressionNode.IfFalse.ShouldEqual(ifFalseChanged);
            expressionNode.IfTrue.ShouldEqual(ifTrueChanged);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void UpdateMetadataShouldCheckMetadataEquality(bool equal)
        {
            var condition = new ConstantExpressionNode("-");
            var ifTrue = new ConstantExpressionNode("1");
            var ifFalse = new ConstantExpressionNode("2");
            var node = new ConditionExpressionNode(condition, ifTrue, ifFalse, EmptyDictionary);
            if (equal)
                node.UpdateMetadata(EmptyDictionary).ShouldEqual(node, ReferenceEqualityComparer.Instance);
            else
            {
                var metadata = new Dictionary<string, object?> {{"k", null}};
                var updated = (ConditionExpressionNode) node.UpdateMetadata(metadata);
                updated.ShouldNotEqual(node, ReferenceEqualityComparer.Instance);
                updated.Metadata.ShouldEqual(metadata);
                updated.Condition.ShouldEqual(node.Condition);
                updated.IfFalse.ShouldEqual(node.IfFalse);
                updated.IfTrue.ShouldEqual(node.IfTrue);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void GetHashCodeEqualsShouldBeValid(bool withComparer)
        {
            var comparer = withComparer ? new TestExpressionEqualityComparer() : null;
            var exp1 = new ConditionExpressionNode(GetTestEqualityExpression(comparer, 0), GetTestEqualityExpression(comparer, 1), GetTestEqualityExpression(comparer, 2),
                new Dictionary<string, object?> {{"k", null}});
            var exp2 = new ConditionExpressionNode(GetTestEqualityExpression(comparer, 0), GetTestEqualityExpression(comparer, 1), GetTestEqualityExpression(comparer, 2),
                new Dictionary<string, object?> {{"k", null}});
            HashCode.Combine(GetBaseHashCode(exp1), 0, 1, 2).ShouldEqual(exp1.GetHashCode(comparer));
            ((TestExpressionNode) exp1.Condition).GetHashCodeCount.ShouldEqual(1);
            ((TestExpressionNode) exp1.IfTrue).GetHashCodeCount.ShouldEqual(1);
            ((TestExpressionNode) exp1.IfFalse).GetHashCodeCount.ShouldEqual(1);

            exp1.Equals(exp2, comparer).ShouldBeTrue();
            ((TestExpressionNode) exp1.Condition).EqualsCount.ShouldEqual(1);
            ((TestExpressionNode) exp1.IfTrue).EqualsCount.ShouldEqual(1);
            ((TestExpressionNode) exp1.IfFalse).EqualsCount.ShouldEqual(1);

            exp1.Equals(exp2.UpdateMetadata(null), comparer).ShouldBeFalse();
            ((TestExpressionNode) exp1.Condition).EqualsCount.ShouldEqual(1);
            ((TestExpressionNode) exp1.IfTrue).EqualsCount.ShouldEqual(1);
            ((TestExpressionNode) exp1.IfFalse).EqualsCount.ShouldEqual(1);

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
            ((TestExpressionNode) exp1.Condition).EqualsCount.ShouldEqual(1);
            ((TestExpressionNode) exp1.IfTrue).EqualsCount.ShouldEqual(1);
            ((TestExpressionNode) exp1.IfFalse).EqualsCount.ShouldEqual(1);
        }
    }
}