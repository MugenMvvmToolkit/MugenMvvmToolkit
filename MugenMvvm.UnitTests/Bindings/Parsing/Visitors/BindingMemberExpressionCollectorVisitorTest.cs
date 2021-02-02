using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Bindings.Parsing.Expressions;
using MugenMvvm.Bindings.Parsing.Expressions.Binding;
using MugenMvvm.Bindings.Parsing.Visitors;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Bindings.Parsing.Visitors
{
    public class BindingMemberExpressionCollectorVisitorTest : UnitTestBase
    {
        private readonly BindingMemberExpressionCollectorVisitor _visitor;

        public BindingMemberExpressionCollectorVisitorTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _visitor = new BindingMemberExpressionCollectorVisitor();
        }

        [Fact]
        public void CollectShouldCollectBindingMemberExpressions1()
        {
            IExpressionNode expression = new BindingMemberExpressionNode("Test", -1, default, default);
            var array = _visitor.Collect(ref expression, DefaultMetadata).AsList();
            array.Count.ShouldEqual(1);
            array[0].Index.ShouldEqual(0);
            array[0].ShouldEqual(expression);

            var oldExp = expression;
            array = _visitor.Collect(ref expression, DefaultMetadata).AsList();
            ReferenceEquals(oldExp, expression).ShouldBeTrue();
            array.Count.ShouldEqual(1);
            array[0].Index.ShouldEqual(0);
            array[0].ShouldEqual(expression);
        }

        [Fact]
        public void CollectShouldCollectBindingMemberExpressions2()
        {
            var ex1 = new BindingMemberExpressionNode("Test1", -1, default, default);
            var ex2 = new BindingMemberExpressionNode("Test2", -1, default, default);
            var ex3 = new BindingMemberExpressionNode("Test2", -1, default, default);
            IExpressionNode expression = new BinaryExpressionNode(BinaryTokenType.Addition, ex1,
                new BinaryExpressionNode(BinaryTokenType.Division, ex2, new BinaryExpressionNode(BinaryTokenType.Addition, ex1, ex3)));

            var array = _visitor.Collect(ref expression, DefaultMetadata).AsList();
            array.Count.ShouldEqual(3);
            array[0].ShouldEqual(ex1.Update(0, ex1.Flags, ex1.MemberFlags, ex1.ObservableMethodName));
            array[1].ShouldEqual(ex2.Update(1, ex2.Flags, ex2.MemberFlags, ex2.ObservableMethodName));
            array[2].ShouldEqual(ex3.Update(2, ex3.Flags, ex3.MemberFlags, ex3.ObservableMethodName));

            var oldExp = expression;
            array = _visitor.Collect(ref expression, DefaultMetadata).AsList();
            ReferenceEquals(oldExp, expression).ShouldBeTrue();
            array.Count.ShouldEqual(3);
            array[0].ShouldEqual(ex1.Update(0, ex1.Flags, ex1.MemberFlags, ex1.ObservableMethodName));
            array[1].ShouldEqual(ex2.Update(1, ex2.Flags, ex2.MemberFlags, ex2.ObservableMethodName));
            array[2].ShouldEqual(ex3.Update(2, ex3.Flags, ex3.MemberFlags, ex3.ObservableMethodName));
        }
    }
}