using System.Collections.Generic;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Binding.Parsing.Expressions;
using MugenMvvm.UnitTest.Binding.Parsing.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Parsing.Expressions
{
    public class UnaryExpressionNodeTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void ConstructorShouldInitializeValues()
        {
            var target = new ConstantExpressionNode("1");
            var exp = new UnaryExpressionNode(UnaryTokenType.BitwiseNegation, target);
            exp.ExpressionType.ShouldEqual(ExpressionNodeType.Unary);
            exp.Operand.ShouldEqual(target);
            exp.Token.ShouldEqual(UnaryTokenType.BitwiseNegation);
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
            var exp = new UnaryExpressionNode(UnaryTokenType.BitwiseNegation, target);

            var result = isPostOrder ? new IExpressionNode[] { target, exp } : new IExpressionNode[] { exp, target };
            exp.Accept(testExpressionVisitor, DefaultMetadata).ShouldEqual(exp);
            result.SequenceEqual(nodes).ShouldBeTrue();
        }


        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void AcceptShouldCreateNewNode1(bool isPostOrder)
        {
            var target = new ConstantExpressionNode("1");
            var targetChanged = new ConstantExpressionNode("1-");
            var testExpressionVisitor = new TestExpressionVisitor
            {
                Visit = (node, context) =>
                {
                    if (node == target)
                        return targetChanged;
                    return node;
                },
                IsPostOrder = isPostOrder
            };
            var exp = new UnaryExpressionNode(UnaryTokenType.BitwiseNegation, target);
            var expressionNode = (UnaryExpressionNode)exp.Accept(testExpressionVisitor, DefaultMetadata);
            expressionNode.ShouldNotEqual(exp);
            expressionNode.Operand.ShouldEqual(targetChanged);
            expressionNode.Token.ShouldEqual(UnaryTokenType.BitwiseNegation);
        }

        [Fact]
        public void AcceptShouldCreateNewNode2()
        {
            var target = new ConstantExpressionNode("1");
            var testExpressionVisitor = new TestExpressionVisitor
            {
                Visit = (node, context) => target
            };
            new UnaryExpressionNode(UnaryTokenType.BitwiseNegation, target).Accept(testExpressionVisitor, DefaultMetadata).ShouldEqual(target);
        }


        [Fact]
        public void GetShouldReturnMacrosMembers()
        {
            UnaryExpressionNode.Get(UnaryTokenType.StaticExpression, MemberExpressionNode.Self).ShouldEqual(UnaryExpressionNode.TargetMacros);
            UnaryExpressionNode.Get(UnaryTokenType.DynamicExpression, MemberExpressionNode.Self).ShouldEqual(UnaryExpressionNode.TargetMacros);
            UnaryExpressionNode.Get(UnaryTokenType.StaticExpression, new MemberExpressionNode(null, MacrosConstant.This)).ShouldEqual(UnaryExpressionNode.TargetMacros);
            UnaryExpressionNode.Get(UnaryTokenType.DynamicExpression, new MemberExpressionNode(null, MacrosConstant.This)).ShouldEqual(UnaryExpressionNode.TargetMacros);
            UnaryExpressionNode.Get(UnaryTokenType.StaticExpression, new MemberExpressionNode(null, MacrosConstant.Target)).ShouldEqual(UnaryExpressionNode.TargetMacros);
            UnaryExpressionNode.Get(UnaryTokenType.DynamicExpression, new MemberExpressionNode(null, MacrosConstant.Target)).ShouldEqual(UnaryExpressionNode.TargetMacros);
            UnaryExpressionNode.Get(UnaryTokenType.StaticExpression, MemberExpressionNode.Context).ShouldEqual(UnaryExpressionNode.ContextMacros);
            UnaryExpressionNode.Get(UnaryTokenType.DynamicExpression, MemberExpressionNode.Context).ShouldEqual(UnaryExpressionNode.ContextMacros);
            UnaryExpressionNode.Get(UnaryTokenType.StaticExpression, MemberExpressionNode.Source).ShouldEqual(UnaryExpressionNode.SourceMacros);
            UnaryExpressionNode.Get(UnaryTokenType.DynamicExpression, MemberExpressionNode.Source).ShouldEqual(UnaryExpressionNode.SourceMacros);
            UnaryExpressionNode.Get(UnaryTokenType.StaticExpression, MemberExpressionNode.EventArgs).ShouldEqual(UnaryExpressionNode.EventArgsMacros);
            UnaryExpressionNode.Get(UnaryTokenType.DynamicExpression, MemberExpressionNode.EventArgs).ShouldEqual(UnaryExpressionNode.EventArgsMacros);
            UnaryExpressionNode.Get(UnaryTokenType.StaticExpression, MemberExpressionNode.Binding).ShouldEqual(UnaryExpressionNode.BindingMacros);
            UnaryExpressionNode.Get(UnaryTokenType.DynamicExpression, MemberExpressionNode.Binding).ShouldEqual(UnaryExpressionNode.BindingMacros);
            UnaryExpressionNode.Get(UnaryTokenType.StaticExpression, MemberExpressionNode.Action).ShouldEqual(UnaryExpressionNode.ActionMacros);
            UnaryExpressionNode.Get(UnaryTokenType.DynamicExpression, MemberExpressionNode.Action).ShouldEqual(UnaryExpressionNode.ActionMacros);

            var node = UnaryExpressionNode.Get(UnaryTokenType.LogicalNegation, MemberExpressionNode.Source);
            node.ShouldNotEqual(UnaryExpressionNode.SourceMacros);
            node.Operand.ShouldEqual(MemberExpressionNode.Source);
            node.Token.ShouldEqual(UnaryTokenType.LogicalNegation);
        }

        #endregion
    }
}