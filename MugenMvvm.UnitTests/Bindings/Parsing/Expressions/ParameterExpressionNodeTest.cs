using System.Collections.Generic;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Bindings.Parsing.Expressions;
using MugenMvvm.UnitTests.Bindings.Parsing.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Parsing.Expressions
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
            exp.ToString().ShouldEqual(Name);
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
            result.ShouldEqual(nodes);
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