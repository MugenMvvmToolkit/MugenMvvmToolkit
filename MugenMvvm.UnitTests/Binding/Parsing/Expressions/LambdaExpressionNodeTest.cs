using System.Collections.Generic;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Binding.Parsing.Expressions;
using MugenMvvm.UnitTests.Binding.Parsing.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Binding.Parsing.Expressions
{
    public class LambdaExpressionNodeTest : UnitTestBase
    {
        #region Methods

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
            var arg1 = new ParameterExpressionNode("2");
            var arg2 = new ParameterExpressionNode("3");
            var exp = new LambdaExpressionNode(target, new[] {arg1, arg2});

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
            var arg1 = new ParameterExpressionNode("2");
            var arg2 = new ParameterExpressionNode("3");

            var targetChanged = new ConstantExpressionNode("1-");
            var arg1Changed = new ParameterExpressionNode("2-");
            var arg2Changed = new ParameterExpressionNode("3-");
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
            var exp = new LambdaExpressionNode(target, new[] {arg1, arg2});
            var expressionNode = (LambdaExpressionNode) exp.Accept(testExpressionVisitor, DefaultMetadata);
            expressionNode.ShouldNotEqual(exp);
            expressionNode.Body.ShouldEqual(targetChanged);
            expressionNode.Parameters.ShouldEqual(new[] {arg1Changed, arg2Changed});
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

        #endregion
    }
}