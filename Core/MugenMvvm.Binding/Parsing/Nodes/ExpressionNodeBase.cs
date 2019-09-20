using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Nodes;

namespace MugenMvvm.Binding.Parsing.Nodes
{
    public abstract class ExpressionNodeBase : IExpressionNode
    {
        #region Fields

        private bool _updating;

        #endregion

        #region Properties

        public abstract ExpressionNodeType NodeType { get; }

        #endregion

        #region Implementation of interfaces

        public IExpressionNode Accept(IExpressionVisitor visitor)
        {
            Should.NotBeNull(visitor, nameof(visitor));
            IExpressionNode? node;
            bool changed = false;
            if (!visitor.IsPostOrder)
            {
                node = VisitWithCheck(visitor, this, true, ref changed);
                if (changed)
                    return node;
            }

            try
            {
                if (_updating)
                    return this;
                _updating = true;
                node = VisitInternal(visitor);
                if (visitor.IsPostOrder)
                    return VisitWithCheck(visitor, node, true, ref changed);
                return node;
            }
            finally
            {
                _updating = false;
            }
        }

        #endregion

        #region Methods

        protected abstract IExpressionNode VisitInternal(IExpressionVisitor visitor);

        protected T VisitWithCheck<T>(IExpressionVisitor visitor, T node, bool notNull, ref bool changed)
            where T : class, IExpressionNode
        {
            var result = ReferenceEquals(this, node) ? visitor.Visit(node) : node.Accept(visitor);
            if (!changed && result != node)
                changed = true;
            //            if (notNull && result == null)//todo add
            //                throw BindingExceptionManager.ExpressionNodeCannotBeNull(GetType());
            return (T)result!;
        }

        #endregion
    }
}