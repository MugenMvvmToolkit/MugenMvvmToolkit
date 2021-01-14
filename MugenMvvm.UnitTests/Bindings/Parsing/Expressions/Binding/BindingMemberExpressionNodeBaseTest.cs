using System.Collections.Generic;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Bindings.Parsing.Expressions;
using MugenMvvm.Bindings.Parsing.Expressions.Binding;
using MugenMvvm.Enums;
using MugenMvvm.UnitTests.Bindings.Parsing.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Parsing.Expressions.Binding
{
    public abstract class BindingMemberExpressionNodeBaseTest<T> : UnitTestBase
        where T : BindingMemberExpressionNodeBase<T>
    {
        protected const string Path = "Path";
        protected const string ResourceName = "R";

        [Fact]
        public void AcceptShouldCreateNewNode2()
        {
            var newNode = new ParameterExpressionNode("");
            var visitor = new TestExpressionVisitor
            {
                Visit = (_, _) => newNode
            };
            GetExpression().Accept(visitor, DefaultMetadata).ShouldEqual(newNode);
        }

        [Fact]
        public void UpdateShouldCreateNewNode()
        {
            var index = 1;
            EnumFlags<BindingMemberExpressionFlags> flags = BindingMemberExpressionFlags.Target;
            EnumFlags<MemberFlags> memberFlags = MemberFlags.Static;
            var observableMethodName = nameof(memberFlags);

            var exp = GetExpression();
            exp.Flags.ShouldNotEqual(flags);
            exp.Index.ShouldNotEqual(index);
            exp.MemberFlags.ShouldNotEqual(memberFlags);
            exp.ObservableMethodName.ShouldNotEqual(observableMethodName);

            var newExp = exp.Update(index, flags, memberFlags, observableMethodName);
            ReferenceEquals(newExp, exp).ShouldBeFalse();
            newExp.Flags.ShouldEqual(flags);
            newExp.Index.ShouldEqual(index);
            newExp.MemberFlags.ShouldEqual(memberFlags);
            newExp.ObservableMethodName.ShouldEqual(observableMethodName);
        }

        [Theory]
        [InlineData(ExpressionTraversalType.InorderValue)]
        [InlineData(ExpressionTraversalType.PreorderValue)]
        [InlineData(ExpressionTraversalType.PostorderValue)]
        public void AcceptShouldVisitWithCorrectOrder(int value)
        {
            var nodes = new List<IExpressionNode>();
            var visitor = new TestExpressionVisitor
            {
                Visit = (node, context) =>
                {
                    nodes.Add(node);
                    context.ShouldEqual(DefaultMetadata);
                    return node;
                },
                TraversalType = ExpressionTraversalType.Get(value)
            };

            var exp = GetExpression();
            var result = new IExpressionNode[] {exp};
            exp.Accept(visitor, DefaultMetadata).ShouldEqual(exp);
            result.ShouldEqual(nodes);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void UpdateMetadataShouldCheckMetadataEquality(bool equal)
        {
            var node = GetExpression(EmptyDictionary);
            if (equal)
                node.UpdateMetadata(EmptyDictionary).ShouldEqual(node, ReferenceEqualityComparer.Instance);
            else
            {
                var metadata = new Dictionary<string, object?> {{"k", null}};
                var updated = (T) node.UpdateMetadata(metadata);
                updated.ShouldNotEqual(node, ReferenceEqualityComparer.Instance);
                updated.ShouldEqual(GetExpression(metadata));
            }
        }

        protected abstract T GetExpression(IReadOnlyDictionary<string, object?>? metadata = null);
    }
}