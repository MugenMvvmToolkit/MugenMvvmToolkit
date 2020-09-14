using System.Collections.Generic;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Binding.Parsing.Expressions;
using MugenMvvm.UnitTests.Binding.Parsing.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Binding.Parsing.Expressions
{
    public class ConditionExpressionNodeTest : UnitTestBase
    {
        #region Methods

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

            var condition = new ConstantExpressionNode("-");
            var ifTrue = new ConstantExpressionNode("1");
            var ifFalse = new ConstantExpressionNode("2");
            var exp = new ConditionExpressionNode(condition, ifTrue, ifFalse);

            var result = isPostOrder ? new IExpressionNode[] {condition, ifTrue, ifFalse, exp} : new IExpressionNode[] {exp, condition, ifTrue, ifFalse};
            exp.Accept(testExpressionVisitor, DefaultMetadata).ShouldEqual(exp);
            result.SequenceEqual(nodes).ShouldBeTrue();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void AcceptShouldCreateNewNode1(bool isPostOrder)
        {
            var condition = new ConstantExpressionNode("-");
            var ifTrue = new ConstantExpressionNode("1");
            var ifFalse = new ConstantExpressionNode("2");
            var conditionChanged = new ConstantExpressionNode("--");
            var ifTrueChanged = new ConstantExpressionNode("1-");
            var ifFalseChanged = new ConstantExpressionNode("2-");
            var testExpressionVisitor = new TestExpressionVisitor
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
                IsPostOrder = isPostOrder
            };
            var exp = new ConditionExpressionNode(condition, ifTrue, ifFalse);
            var expressionNode = (ConditionExpressionNode) exp.Accept(testExpressionVisitor, DefaultMetadata);
            expressionNode.ShouldNotEqual(exp);
            expressionNode.Condition.ShouldEqual(conditionChanged);
            expressionNode.IfFalse.ShouldEqual(ifFalseChanged);
            expressionNode.IfTrue.ShouldEqual(ifTrueChanged);
        }

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

        #endregion
    }
}