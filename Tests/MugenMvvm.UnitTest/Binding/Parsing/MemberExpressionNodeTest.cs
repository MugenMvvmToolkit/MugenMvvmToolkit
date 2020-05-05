using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Binding.Parsing.Expressions;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Parsing
{
    public class MemberExpressionNodeTest : UnitTestBase
    {
        #region Fields

        private const string MemberName = "@4";

        #endregion

        #region Methods

        [Fact]
        public void StaticFieldsShouldBeCorrect()
        {
            MemberExpressionNode.Empty.Target.ShouldBeNull();
            MemberExpressionNode.Empty.Member.ShouldEqual(string.Empty);

            MemberExpressionNode.Source.Target.ShouldBeNull();
            MemberExpressionNode.Source.Member.ShouldEqual(MacrosConstant.Source);

            MemberExpressionNode.Self.Target.ShouldBeNull();
            MemberExpressionNode.Self.Member.ShouldEqual(MacrosConstant.Target);

            MemberExpressionNode.Context.Target.ShouldBeNull();
            MemberExpressionNode.Context.Member.ShouldEqual(MacrosConstant.Context);

            MemberExpressionNode.Binding.Target.ShouldBeNull();
            MemberExpressionNode.Binding.Member.ShouldEqual(MacrosConstant.Binding);

            MemberExpressionNode.Args.Target.ShouldBeNull();
            MemberExpressionNode.Args.Member.ShouldEqual(MacrosConstant.Args);
        }

        [Fact]
        public void GetShouldReturnMacrosMembers()
        {
            MemberExpressionNode.Get(null, MacrosConstant.Self).ShouldEqual(MemberExpressionNode.Self);
            MemberExpressionNode.Get(null, MacrosConstant.This).ShouldEqual(MemberExpressionNode.Self);
            MemberExpressionNode.Get(null, MacrosConstant.Target).ShouldEqual(MemberExpressionNode.Self);
            MemberExpressionNode.Get(null, MacrosConstant.Context).ShouldEqual(MemberExpressionNode.Context);
            MemberExpressionNode.Get(null, MacrosConstant.Source).ShouldEqual(MemberExpressionNode.Source);
            MemberExpressionNode.Get(null, MacrosConstant.Args).ShouldEqual(MemberExpressionNode.Args);
            MemberExpressionNode.Get(null, MacrosConstant.Binding).ShouldEqual(MemberExpressionNode.Binding);

            var node = MemberExpressionNode.Get(ConstantExpressionNode.False, MacrosConstant.Self);
            node.ShouldNotEqual(MemberExpressionNode.Self);
            node.Target.ShouldEqual(ConstantExpressionNode.False);
            node.Member.ShouldEqual(MacrosConstant.Self);
        }

        [Fact]
        public void ConstructorShouldInitializeValues()
        {
            var target = new ConstantExpressionNode("1");
            var exp = new MemberExpressionNode(target, MemberName);
            exp.ExpressionType.ShouldEqual(ExpressionNodeType.Member);
            exp.Target.ShouldEqual(target);
            exp.Member.ShouldEqual(MemberName);
        }

        [Fact]
        public void UpdateTargetShouldCreateNewNode()
        {
            var target = new ConstantExpressionNode("1");
            var newTarget = new ConstantExpressionNode("2");
            var exp = new MemberExpressionNode(target, MemberName);
            exp.UpdateTarget(target).ShouldEqual(exp);

            var newExp = exp.UpdateTarget(newTarget);
            newExp.ShouldNotEqual(exp);
            newExp.ExpressionType.ShouldEqual(ExpressionNodeType.Member);
            newExp.Target.ShouldEqual(newTarget);
            newExp.Member.ShouldEqual(MemberName);
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
            var exp = new MemberExpressionNode(target, MemberName);

            var result = isPostOrder ? new IExpressionNode[] {target, exp} : new IExpressionNode[] {exp, target};
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
            var exp = new MemberExpressionNode(target, MemberName);
            var expressionNode = (MemberExpressionNode) exp.Accept(testExpressionVisitor, DefaultMetadata);
            expressionNode.ShouldNotEqual(exp);
            expressionNode.Target.ShouldEqual(targetChanged);
            expressionNode.Member.ShouldEqual(MemberName);
        }

        [Fact]
        public void AcceptShouldCreateNewNode2()
        {
            var target = new ConstantExpressionNode("1");
            var testExpressionVisitor = new TestExpressionVisitor
            {
                Visit = (node, context) => target
            };
            new MemberExpressionNode(target, MemberName).Accept(testExpressionVisitor, DefaultMetadata).ShouldEqual(target);
        }

        #endregion
    }
}