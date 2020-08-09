using System.Collections.Generic;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Binding.Parsing.Expressions;
using MugenMvvm.UnitTest.Binding.Parsing.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Parsing.Expressions
{
    public class IndexExpressionNodeTest : UnitTestBase
    {
        #region Methods

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
            var newArgs = new IExpressionNode[] {new ConstantExpressionNode("2")};
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
            var exp = new IndexExpressionNode(target, new[] {arg1, arg2});

            var result = isPostOrder ? new IExpressionNode[] {target, arg1, arg2, exp} : new IExpressionNode[] {exp, target, arg1, arg2};
            exp.Accept(testExpressionVisitor, DefaultMetadata).ShouldEqual(exp);
            result.SequenceEqual(nodes).ShouldBeTrue();
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
            var exp = new IndexExpressionNode(target, new[] {arg1, arg2});
            var expressionNode = (IndexExpressionNode) exp.Accept(testExpressionVisitor, DefaultMetadata);
            expressionNode.ShouldNotEqual(exp);
            expressionNode.Target.ShouldEqual(targetChanged);
            expressionNode.Arguments.SequenceEqual(new[] {arg1Changed, arg2Changed}).ShouldBeTrue();
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
            new IndexExpressionNode(target, args).Accept(testExpressionVisitor, DefaultMetadata).ShouldEqual(target);
        }

        #endregion
    }
}