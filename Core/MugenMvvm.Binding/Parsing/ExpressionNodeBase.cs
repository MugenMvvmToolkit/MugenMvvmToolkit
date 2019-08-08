﻿using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Parsing;

namespace MugenMvvm.Binding.Parsing
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
            IExpressionNode node;
            if (!visitor.IsPostOrder)
            {
                node = visitor.Visit(this);
                if (node != this)
                    return node;
            }

            try
            {
                if (_updating)
                    return this;
                _updating = true;
                node = VisitInternal(visitor);
                if (visitor.IsPostOrder)
                    return visitor.Visit(node);
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
            var result = node.Accept(visitor);
            if (!changed && result != node)
                changed = true;
            //            if (notNull && result == null)//todo add
            //                throw BindingExceptionManager.ExpressionNodeCannotBeNull(GetType());
            return (T)result;
        }

        #endregion
    }
}