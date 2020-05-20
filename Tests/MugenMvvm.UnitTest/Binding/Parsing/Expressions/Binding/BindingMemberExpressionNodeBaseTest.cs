using System.Collections.Generic;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Binding.Parsing.Expressions;
using MugenMvvm.Binding.Parsing.Expressions.Binding;
using MugenMvvm.UnitTest.Binding.Parsing.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Parsing.Expressions.Binding
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
            result.SequenceEqual(nodes).ShouldBeTrue();
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