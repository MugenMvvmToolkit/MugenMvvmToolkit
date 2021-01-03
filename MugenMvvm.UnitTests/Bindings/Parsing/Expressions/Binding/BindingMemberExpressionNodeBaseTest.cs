using System.Collections.Generic;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Bindings.Parsing.Expressions;
using MugenMvvm.Bindings.Parsing.Expressions.Binding;
using MugenMvvm.Enums;
using MugenMvvm.UnitTests.Bindings.Parsing.Internal;
using MugenMvvm.UnitTests.Internal.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Parsing.Expressions.Binding
{
    public abstract class BindingMemberExpressionNodeBaseTest<T> : UnitTestBase
        where T : BindingMemberExpressionNodeBase<T>
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
                Visit = (_, _) => newNode
            };
            GetExpression().Accept(testExpressionVisitor, DefaultMetadata).ShouldEqual(newNode);
        }

        [Fact]
        public void UpdateShouldCreateNewNode()
        {
            int index = 1;
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

        #endregion
    }
}