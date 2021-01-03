using System;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Bindings.Parsing.Expressions;
using MugenMvvm.UnitTests.Bindings.Parsing.Internal;
using MugenMvvm.UnitTests.Internal.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Parsing.Expressions
{
    public class MethodCallExpressionNodeTest : UnitTestBase
    {
        #region Fields

        private const string MethodName = "@4";
        private static readonly string[] TypeArgs = {"te", "tes"};

        #endregion

        #region Methods

        [Fact]
        public void ConstructorShouldInitializeValues()
        {
            var target = new ConstantExpressionNode("1");
            var args = new IExpressionNode[] {new ConstantExpressionNode("2")};
            var exp = new MethodCallExpressionNode(target, MethodName, args, TypeArgs);
            exp.ExpressionType.ShouldEqual(ExpressionNodeType.MethodCall);
            exp.Target.ShouldEqual(target);
            exp.Arguments.ShouldEqual(args);
            exp.TypeArgs.ShouldEqual(TypeArgs);
            exp.Method.ShouldEqual(MethodName);
            exp.ToString().ShouldEqual("\"1\".@4<te, tes>(\"2\")");
        }

        [Fact]
        public void UpdateArgumentsShouldCreateNewNode()
        {
            var target = new ConstantExpressionNode("1");
            var args = new IExpressionNode[] {new ConstantExpressionNode("2")};
            var newArgs = new IExpressionNode[] {new ConstantExpressionNode("3")};
            var exp = new MethodCallExpressionNode(target, MethodName, args, TypeArgs);
            exp.UpdateArguments(args).ShouldEqual(exp);

            var newExp = exp.UpdateArguments(newArgs);
            newExp.ShouldNotEqual(exp);
            newExp.ExpressionType.ShouldEqual(ExpressionNodeType.MethodCall);
            newExp.Target.ShouldEqual(target);
            newExp.Arguments.ShouldEqual(newArgs);
            newExp.TypeArgs.ShouldEqual(TypeArgs);
            newExp.Method.ShouldEqual(MethodName);
        }

        [Fact]
        public void UpdateTargetShouldCreateNewNode()
        {
            var target = new ConstantExpressionNode("1");
            var args = new IExpressionNode[] {new ConstantExpressionNode("2")};
            var newTarget = new ConstantExpressionNode("2");
            var exp = new MethodCallExpressionNode(target, MethodName, args, TypeArgs);
            exp.UpdateTarget(target).ShouldEqual(exp);

            var newExp = exp.UpdateTarget(newTarget);
            newExp.ShouldNotEqual(exp);
            newExp.ExpressionType.ShouldEqual(ExpressionNodeType.MethodCall);
            newExp.Target.ShouldEqual(newTarget);
            newExp.Arguments.ShouldEqual(args);
            newExp.TypeArgs.ShouldEqual(TypeArgs);
            newExp.Method.ShouldEqual(MethodName);
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

            var target = new ConstantExpressionNode("1");
            var arg1 = new ConstantExpressionNode("2");
            var arg2 = new ConstantExpressionNode("3");
            var exp = new MethodCallExpressionNode(target, MethodName, new[] {arg1, arg2}, TypeArgs);

            var result = isPostOrder ? new IExpressionNode[] {target, arg1, arg2, exp} : new IExpressionNode[] {exp, target, arg1, arg2};
            exp.Accept(testExpressionVisitor, DefaultMetadata).ShouldEqual(exp);
            result.ShouldEqual(nodes);
        }


        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void AcceptShouldCreateNewNode1(bool isPostOrder)
        {
            var target = new ConstantExpressionNode("1");
            var arg1 = new ConstantExpressionNode("2");
            var arg2 = new ConstantExpressionNode("3");

            var targetChanged = new ConstantExpressionNode("1-");
            var arg1Changed = new ConstantExpressionNode("2-");
            var arg2Changed = new ConstantExpressionNode("3-");
            var testExpressionVisitor = new TestExpressionVisitor
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
                IsPostOrder = isPostOrder
            };
            var exp = new MethodCallExpressionNode(target, MethodName, new[] {arg1, arg2}, TypeArgs);
            var expressionNode = (MethodCallExpressionNode) exp.Accept(testExpressionVisitor, DefaultMetadata);
            expressionNode.ShouldNotEqual(exp);
            expressionNode.Target.ShouldEqual(targetChanged);
            expressionNode.Arguments.ShouldEqual(new[] {arg1Changed, arg2Changed});
            expressionNode.TypeArgs.ShouldEqual(TypeArgs);
            expressionNode.Method.ShouldEqual(MethodName);
        }

        [Fact]
        public void AcceptShouldCreateNewNode2()
        {
            var target = new ConstantExpressionNode("1");
            var args = new IExpressionNode[] {new ConstantExpressionNode("2")};
            var testExpressionVisitor = new TestExpressionVisitor
            {
                Visit = (node, context) => target
            };
            new MethodCallExpressionNode(target, MethodName, args).Accept(testExpressionVisitor, DefaultMetadata).ShouldEqual(target);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void UpdateMetadataShouldCheckMetadataEquality(bool equal)
        {
            var target = new ConstantExpressionNode("1");
            var args = new IExpressionNode[] {new ConstantExpressionNode("2")};
            var node = new MethodCallExpressionNode(target, MethodName, args, TypeArgs, EmptyDictionary);
            if (equal)
                node.UpdateMetadata(EmptyDictionary).ShouldEqual(node, ReferenceEqualityComparer.Instance);
            else
            {
                var metadata = new Dictionary<string, object?> {{"k", null}};
                var updated = (MethodCallExpressionNode) node.UpdateMetadata(metadata);
                updated.ShouldNotEqual(node, ReferenceEqualityComparer.Instance);
                updated.Metadata.ShouldEqual(metadata);
                updated.Target.ShouldEqual(node.Target);
                updated.Arguments.ShouldEqual(node.Arguments);
                updated.Method.ShouldEqual(node.Method);
                updated.TypeArgs.ShouldEqual(node.TypeArgs);
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
            var exp1 = new MethodCallExpressionNode(hasTarget ? GetTestEqualityExpression(comparer, 1) : null, "M", new IExpressionNode[] {GetTestEqualityExpression(comparer, 2), GetTestEqualityExpression(comparer, 3)},
                TypeArgs, new Dictionary<string, object?> {{"k", null}});
            var exp2 = new MethodCallExpressionNode(hasTarget ? GetTestEqualityExpression(comparer, 1) : null, "M", new IExpressionNode[] {GetTestEqualityExpression(comparer, 2), GetTestEqualityExpression(comparer, 3)},
                TypeArgs, new Dictionary<string, object?> {{"k", null}});
            ;
            if (hasTarget)
            {
                HashCode.Combine(GetBaseHashCode(exp1), exp1.Method, exp1.Arguments.Count, TypeArgs.Length, 1).ShouldEqual(exp1.GetHashCode(comparer));
                ((TestExpressionNode) exp1.Target!).GetHashCodeCount.ShouldEqual(1);
            }
            else
                HashCode.Combine(GetBaseHashCode(exp1), exp1.Method, exp1.Arguments.Count, TypeArgs.Length).ShouldEqual(exp1.GetHashCode(comparer));

            exp1.Arguments.Cast<TestExpressionNode>().All(node => node.GetHashCodeCount == 0).ShouldBeTrue();

            exp1.Equals(exp2, comparer).ShouldBeTrue();
            ((TestExpressionNode?) exp1.Target)?.EqualsCount.ShouldEqual(1);
            exp1.Arguments.Cast<TestExpressionNode>().All(node => node.EqualsCount == 1).ShouldBeTrue();

            exp1.Equals(exp2.UpdateMetadata(null), comparer).ShouldBeFalse();
            exp1.Equals(new MethodCallExpressionNode(hasTarget ? GetTestEqualityExpression(comparer, 1) : null, "M", new IExpressionNode[] {GetTestEqualityExpression(comparer, 2), GetTestEqualityExpression(comparer, 3)},
                null, new Dictionary<string, object?> {{"k", null}}), comparer).ShouldBeFalse();
            ((TestExpressionNode?) exp1.Target)?.EqualsCount.ShouldEqual(1);
            exp1.Arguments.Cast<TestExpressionNode>().All(node => node.EqualsCount == 1).ShouldBeTrue();

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
            exp1.Arguments.Cast<TestExpressionNode>().All(node => node.EqualsCount == 1).ShouldBeTrue();
        }

        #endregion
    }
}