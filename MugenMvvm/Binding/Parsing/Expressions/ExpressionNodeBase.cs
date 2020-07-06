using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Parsing.Expressions
{
    public abstract class ExpressionNodeBase : IExpressionNode
    {
        #region Properties

        public abstract ExpressionNodeType ExpressionType { get; }

        #endregion

        #region Implementation of interfaces

        public IExpressionNode Accept(IExpressionVisitor visitor, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(visitor, nameof(visitor));
            var node = AcceptInternal(visitor, metadata);
            if (node == this)
                return node;
            return node.Accept(visitor, metadata);
        }

        #endregion

        #region Methods

        private IExpressionNode AcceptInternal(IExpressionVisitor visitor, IReadOnlyMetadataContext? metadata)
        {
            IExpressionNode? node;
            var changed = false;
            if (!visitor.IsPostOrder)
            {
                node = VisitWithCheck(visitor, this, true, ref changed, metadata);
                if (changed)
                    return node;
            }

            node = Visit(visitor, metadata);
            if (visitor.IsPostOrder)
                return VisitWithCheck(visitor, node, true, ref changed, metadata);
            return node;
        }

        protected abstract IExpressionNode Visit(IExpressionVisitor visitor, IReadOnlyMetadataContext? metadata);

        protected T VisitWithCheck<T>(IExpressionVisitor visitor, T node, bool notNull, ref bool changed, IReadOnlyMetadataContext? metadata)
            where T : class, IExpressionNode
        {
            var result = this == node ? visitor.Visit(node, metadata) : node.Accept(visitor, metadata);
            if (!changed && result != node)
                changed = true;
            if (notNull && result == null)
                BindingExceptionManager.ThrowExpressionNodeCannotBeNull(GetType());
            return (T)result!;
        }

        protected IReadOnlyList<T> VisitWithCheck<T>(IExpressionVisitor visitor, IReadOnlyList<T> nodes, ref bool changed, IReadOnlyMetadataContext? metadata)
            where T : class, IExpressionNode
        {
            T[]? newArgs = null;
            for (var i = 0; i < nodes.Count; i++)
            {
                var itemsChanged = false;
                var node = VisitWithCheck(visitor, nodes[i], true, ref itemsChanged, metadata);
                if (!itemsChanged)
                    continue;
                if (newArgs == null)
                    newArgs = nodes.ToArray();
                newArgs[i] = node;
            }

            if (!changed && newArgs != null)
                changed = true;
            return newArgs ?? nodes;
        }

        #endregion
    }
}