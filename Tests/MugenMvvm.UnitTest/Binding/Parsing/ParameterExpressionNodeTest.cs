using System;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Binding.Parsing.Expressions;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Parsing
{
    public class ParameterExpressionNodeTest : UnitTestBase
    {
        #region Fields

        private const string Name = "test";

        #endregion

        #region Methods

        [Fact]
        public void ConstructorShouldInitializeValues()
        {
            var exp = new ParameterExpressionNode(Name);
            exp.ExpressionType.ShouldEqual(ExpressionNodeType.Parameter);
            exp.Name.ShouldEqual(Name);
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

            var exp = new ParameterExpressionNode(Name);
            var result = new IExpressionNode[] {exp};
            exp.Accept(testExpressionVisitor, DefaultMetadata).ShouldEqual(exp);
            result.SequenceEqual(nodes).ShouldBeTrue();
        }

        [Fact]
        public void AcceptShouldCreateNewNode2()
        {
            var newNode = new ParameterExpressionNode(Name);
            var testExpressionVisitor = new TestExpressionVisitor
            {
                Visit = (node, context) => newNode
            };
            new ParameterExpressionNode("1").Accept(testExpressionVisitor, DefaultMetadata).ShouldEqual(newNode);
        }

        #endregion
    }
}