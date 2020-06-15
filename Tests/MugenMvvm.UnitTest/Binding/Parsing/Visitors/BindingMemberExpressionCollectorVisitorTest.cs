using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Parsing.Expressions;
using MugenMvvm.Binding.Parsing.Expressions.Binding;
using MugenMvvm.Binding.Parsing.Visitors;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Parsing.Visitors
{
    public class BindingMemberExpressionCollectorVisitorTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void CollectShouldCollectBindingMemberExpressions1()
        {
            var expressionNode = new BindingMemberExpressionNode("Test");
            var visitor = new BindingMemberExpressionCollectorVisitor();
            var array = visitor.Collect(expressionNode, DefaultMetadata).AsList();
            array.Length.ShouldEqual(1);
            array[0].ShouldEqual(expressionNode);
            expressionNode.Index.ShouldEqual(0);
        }

        [Fact]
        public void CollectShouldCollectBindingMemberExpressions2()
        {
            var ex1 = new BindingMemberExpressionNode("Test1");
            var ex2 = new BindingMemberExpressionNode("Test2");
            var ex3 = new BindingMemberExpressionNode("Test2");
            var expression = new BinaryExpressionNode(BinaryTokenType.Addition, ex1, new BinaryExpressionNode(BinaryTokenType.Division, ex2, new BinaryExpressionNode(BinaryTokenType.Addition, ex1, ex3)));
            var visitor = new BindingMemberExpressionCollectorVisitor();
            var array = visitor.Collect(expression, DefaultMetadata).AsList();
            array.Length.ShouldEqual(3);
            array[0].ShouldEqual(ex1);
            array[1].ShouldEqual(ex2);
            array[2].ShouldEqual(ex3);
            ex1.Index.ShouldEqual(0);
            ex2.Index.ShouldEqual(1);
            ex3.Index.ShouldEqual(2);
        }

        #endregion
    }
}