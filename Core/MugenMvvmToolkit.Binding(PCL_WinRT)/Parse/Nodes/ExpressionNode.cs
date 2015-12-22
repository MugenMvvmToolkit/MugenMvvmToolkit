#region Copyright

// ****************************************************************************
// <copyright file="ExpressionNode.cs">
// Copyright (c) 2012-2015 Vyacheslav Volkov
// </copyright>
// ****************************************************************************
// <author>Vyacheslav Volkov</author>
// <email>vvs0205@outlook.com</email>
// <project>MugenMvvmToolkit</project>
// <web>https://github.com/MugenMvvmToolkit/MugenMvvmToolkit</web>
// <license>
// See license.txt in this solution or http://opensource.org/licenses/MS-PL
// </license>
// ****************************************************************************

#endregion

using System.Threading;
using MugenMvvmToolkit.Binding.Interfaces.Parse;
using MugenMvvmToolkit.Binding.Interfaces.Parse.Nodes;
using MugenMvvmToolkit.Binding.Models;

namespace MugenMvvmToolkit.Binding.Parse.Nodes
{
    public abstract class ExpressionNode : IExpressionNode
    {
        #region Fields

        private int _state;
        private readonly ExpressionNodeType _nodeType;

        #endregion

        #region Constructors

        protected ExpressionNode(ExpressionNodeType type)
        {
            _nodeType = type;
        }

        #endregion

        #region Implementation of IExpressionNode

        public virtual ExpressionNodeType NodeType => _nodeType;

        public IExpressionNode Accept(IExpressionVisitor visitor)
        {
            Should.NotBeNull(visitor, nameof(visitor));
            var isPostOrder = visitor.IsPostOrder;
            if (!isPostOrder)
            {
                IExpressionNode result = visitor.Visit(this);
                if (result != this)
                    return result;
            }
            try
            {
                if (Interlocked.Exchange(ref _state, 1) == 1)
                    return this;
                AcceptInternal(visitor);
                if (isPostOrder)
                {
                    IExpressionNode result = visitor.Visit(this);
                    if (result != this)
                        return result;
                }
                return this;
            }
            finally
            {
                Interlocked.Exchange(ref _state, 0);
            }
        }

        public IExpressionNode Clone()
        {
            return CloneInternal();
        }

        #endregion

        #region Methods

        protected abstract void AcceptInternal(IExpressionVisitor visitor);

        protected abstract IExpressionNode CloneInternal();

        protected T AcceptWithCheck<T>(IExpressionVisitor visitor, T node, bool notNull)
            where T : IExpressionNode
        {
            IExpressionNode result = node.Accept(visitor);
            if (notNull && result == null)
                throw BindingExceptionManager.ExpressionNodeCannotBeNull(GetType());
            return (T)result;
        }

        #endregion
    }
}
