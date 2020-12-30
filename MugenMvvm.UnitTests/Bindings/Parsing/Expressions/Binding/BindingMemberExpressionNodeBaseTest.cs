using System.Collections.Generic;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Bindings.Parsing.Expressions;
using MugenMvvm.Bindings.Parsing.Expressions.Binding;
using MugenMvvm.UnitTests.Bindings.Parsing.Internal;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Parsing.Expressions.Binding
{
    public abstract class BindingMemberExpressionNodeBaseTest : UnitTestBase
    {
        #region Fields

        protected const string Path = "Path";
        protected const string ResourceName = "R";

        #endregion

        #region Methods

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

            var exp = GetExpression();
            var result = new IExpressionNode[] {exp};
            exp.Accept(testExpressionVisitor, DefaultMetadata).ShouldEqual(exp);
            result.ShouldEqual(nodes);
        }

        [Fact]
        public void AcceptShouldCreateNewNode2()
        {
            var newNode = new ParameterExpressionNode("");
            var testExpressionVisitor = new TestExpressionVisitor
            {
                Visit = (node, context) => newNode
            };
            GetExpression().Accept(testExpressionVisitor, DefaultMetadata).ShouldEqual(newNode);
        }

        protected abstract BindingMemberExpressionNodeBase GetExpression();

        #endregion
    }
}