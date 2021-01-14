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
    public class LambdaExpressionNodeTest : UnitTestBase
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
            var arg1 = new ParameterExpressionNode("2");
            var arg2 = new ParameterExpressionNode("3");
            var exp = new LambdaExpressionNode(target, new[] {arg1, arg2});

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
            var arg1 = new ParameterExpressionNode("2");
            var arg2 = new ParameterExpressionNode("3");

            var targetChanged = new ConstantExpressionNode("1-");
            var arg1Changed = new ParameterExpressionNode("2-");
            var arg2Changed = new ParameterExpressionNode("3-");
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
            var exp = new LambdaExpressionNode(target, new[] {arg1, arg2});
            var expressionNode = (LambdaExpressionNode) exp.Accept(visitor, DefaultMetadata);
            expressionNode.ShouldNotEqual(exp);
            expressionNode.Body.ShouldEqual(targetChanged);
            expressionNode.Parameters.AsList().ShouldEqual(new[] {arg1Changed, arg2Changed});
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void UpdateMetadataShouldCheckMetadataEquality(bool equal)
        {
            var target = new ConstantExpressionNode("1");
            var args = new IParameterExpressionNode[] {new ParameterExpressionNode("Test")};
            var node = new LambdaExpressionNode(target, args, EmptyDictionary);
            if (equal)
                node.UpdateMetadata(EmptyDictionary).ShouldEqual(node, ReferenceEqualityComparer.Instance);
            else
            {
                var metadata = new Dictionary<string, object?> {{"k", null}};
                var updated = (LambdaExpressionNode) node.UpdateMetadata(metadata);
                updated.ShouldNotEqual(node, ReferenceEqualityComparer.Instance);
                updated.Metadata.ShouldEqual(metadata);
                updated.Body.ShouldEqual(node.Body);
                updated.Parameters.ShouldEqual(node.Parameters);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void GetHashCodeEqualsShouldBeValid(bool withComparer)
        {
            var comparer = withComparer ? new TestExpressionEqualityComparer() : null;
            var exp1 = new LambdaExpressionNode(GetTestEqualityExpression(comparer, 1), new IParameterExpressionNode[] {new ParameterExpressionNode("")},
                new Dictionary<string, object?> {{"k", null}});
            var exp2 = new LambdaExpressionNode(GetTestEqualityExpression(comparer, 1), new IParameterExpressionNode[] {new ParameterExpressionNode("")},
                new Dictionary<string, object?> {{"k", null}});
            ;
            HashCode.Combine(GetBaseHashCode(exp1), exp1.Parameters.Count, 1).ShouldEqual(exp1.GetHashCode(comparer));
            ((TestExpressionNode) exp1.Body).GetHashCodeCount.ShouldEqual(1);

            exp1.Equals(exp2, comparer).ShouldBeTrue();
            ((TestExpressionNode) exp1.Body).EqualsCount.ShouldEqual(1);

            exp1.Equals(exp2.UpdateMetadata(null), comparer).ShouldBeFalse();
            ((TestExpressionNode) exp1.Body).EqualsCount.ShouldEqual(1);

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
            ((TestExpressionNode) exp1.Body).EqualsCount.ShouldEqual(1);
        }

        [Fact]
        public void AcceptShouldCreateNewNode2()
        {
            var target = new ConstantExpressionNode("1");
            var args = new[] {new ParameterExpressionNode("2")};
            var testExpressionVisitor = new TestExpressionVisitor
            {
                Visit = (node, context) => target
            };
            new LambdaExpressionNode(target, args).Accept(testExpressionVisitor, DefaultMetadata).ShouldEqual(target);
        }

        [Fact]
        public void ConstructorShouldInitializeValues()
        {
            var target = new ConstantExpressionNode("1");
            var args = new IParameterExpressionNode[] {new ParameterExpressionNode("Test")};
            var exp = new LambdaExpressionNode(target, args);
            exp.ExpressionType.ShouldEqual(ExpressionNodeType.Lambda);
            exp.Body.ShouldEqual(target);
            exp.Parameters.ShouldEqual(args);
            exp.ToString().ShouldEqual("(Test) => \"1\"");
        }
    }
}