using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Parsing.Expressions
{
    public abstract class ExpressionNodeBase : IExpressionNode
    {
        #region Properties

        public abstract ExpressionNodeType NodeType { get; }

        #endregion

        #region Implementation of interfaces

        public IExpressionNode Accept(IExpressionVisitor visitor, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(visitor, nameof(visitor));
            IExpressionNode? node;
            var changed = false;
            if (!visitor.IsPostOrder)
            {
                node = VisitWithCheck(visitor, this, true, ref changed, metadata);
                if (changed)
                    return node;
            }

            node = VisitInternal(visitor, metadata);
            if (visitor.IsPostOrder)
                return VisitWithCheck(visitor, node, true, ref changed, metadata);
            return node;
        }

        #endregion

        #region Methods

        protected abstract IExpressionNode VisitInternal(IExpressionVisitor visitor, IReadOnlyMetadataContext? metadata);

        protected T VisitWithCheck<T>(IExpressionVisitor visitor, T node, bool notNull, ref bool changed, IReadOnlyMetadataContext? metadata)
            where T : class, IExpressionNode
        {
            var result = ReferenceEquals(this, node) ? visitor.Visit(node, metadata) : node.Accept(visitor, metadata);
            if (!changed && result != node)
                changed = true;
            if (notNull && result == null)
                BindingExceptionManager.ThrowExpressionNodeCannotBeNull(GetType());
            return (T)result!;
        }

        #endregion
    }
}