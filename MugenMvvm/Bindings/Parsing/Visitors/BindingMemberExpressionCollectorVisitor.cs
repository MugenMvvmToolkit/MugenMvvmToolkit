using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Parsing;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Bindings.Parsing.Visitors
{
    public sealed class BindingMemberExpressionCollectorVisitor : IExpressionVisitor
    {
        private readonly List<(IBindingMemberExpressionNode, IBindingMemberExpressionNode)> _members;

        public BindingMemberExpressionCollectorVisitor()
        {
            _members = new List<(IBindingMemberExpressionNode, IBindingMemberExpressionNode)>(4);
        }

        ExpressionTraversalType IExpressionVisitor.TraversalType => ExpressionTraversalType.Preorder;

        public ItemOrIReadOnlyList<IBindingMemberExpressionNode> Collect([NotNullIfNotNull("expression")] ref IExpressionNode? expression,
            IReadOnlyMetadataContext? metadata = null)
        {
            if (expression == null)
                return default;

            expression = expression.Accept(this, metadata);
            var count = _members.Count;
            if (count == 0)
                return default;
            if (count == 1)
            {
                var r = _members[0].Item2;
                _members.Clear();
                return new ItemOrIReadOnlyList<IBindingMemberExpressionNode>(r);
            }

            var nodes = new IBindingMemberExpressionNode[count];
            for (var i = 0; i < nodes.Length; i++)
                nodes[i] = _members[i].Item2;
            _members.Clear();
            return nodes;
        }

        private IBindingMemberExpressionNode? TryGet(IBindingMemberExpressionNode node)
        {
            for (var i = 0; i < _members.Count; i++)
            {
                var tuple = _members[i];
                if (ReferenceEquals(tuple.Item1, node) || ReferenceEquals(tuple.Item2, node))
                    return tuple.Item2;
            }

            return null;
        }

        IExpressionNode IExpressionVisitor.Visit(IExpressionNode expression, IReadOnlyMetadataContext? metadata)
        {
            if (expression is not IBindingMemberExpressionNode bindingMember)
                return expression;

            var updated = TryGet(bindingMember);
            if (updated == null)
            {
                updated = bindingMember.Update(_members.Count, bindingMember.Flags, bindingMember.MemberFlags, bindingMember.ObservableMethodName);
                _members.Add((bindingMember, updated));
            }

            return updated;
        }
    }
}