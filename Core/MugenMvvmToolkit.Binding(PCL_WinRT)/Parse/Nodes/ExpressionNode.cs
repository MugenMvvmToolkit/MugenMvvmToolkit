#region Copyright
// ****************************************************************************
// <copyright file="ExpressionNode.cs">
// Copyright © Vyacheslav Volkov 2012-2014
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
    /// <summary>
    ///     Provides the base class from which the classes that represent expression tree nodes are derived.
    /// </summary>
    public abstract class ExpressionNode : IExpressionNode
    {
        #region Fields

        private int _state;
        private readonly ExpressionNodeType _nodeType;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ExpressionNode" /> class.
        /// </summary>
        protected ExpressionNode(ExpressionNodeType type)
        {
            _nodeType = type;
        }

        #endregion

        #region Implementation of IExpressionNode

        /// <summary>
        ///     Gets the node type of this <see cref="IExpressionNode" />.
        /// </summary>
        public virtual ExpressionNodeType NodeType
        {
            get { return _nodeType; }
        }

        /// <summary>
        ///     Dispatches to the specific visit method for this node type.
        /// </summary>
        /// <param name="visitor">The visitor to visit this node with.</param>
        /// <returns>
        ///     The result of visiting this node.
        /// </returns>
        public IExpressionNode Accept(IExpressionVisitor visitor)
        {
            Should.NotBeNull(visitor, "visitor");
            IExpressionNode result = visitor.Visit(this);
            if (result != this)
                return result;
            try
            {
                if (Interlocked.Exchange(ref _state, 1) == 1)
                    return this;
                AcceptInternal(visitor);
                return this;
            }
            finally
            {
                Interlocked.Exchange(ref _state, 0);
            }
        }

        /// <summary>
        ///     Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        ///     A new object that is a copy of this instance.
        /// </returns>
        public IExpressionNode Clone()
        {
            return CloneInternal();
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Dispatches to the specific visit method for this node type.
        /// </summary>
        /// <param name="visitor">The visitor to visit this node with.</param>
        protected abstract void AcceptInternal(IExpressionVisitor visitor);

        /// <summary>
        ///     Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        ///     A new object that is a copy of this instance.
        /// </returns>
        protected abstract IExpressionNode CloneInternal();

        /// <summary>
        ///     Dispatches to the specific visit method for this node type.
        /// </summary>
        protected T AcceptWithCheck<T>(IExpressionVisitor visitor, T node, bool notNull)
            where T : IExpressionNode
        {
            IExpressionNode result = node.Accept(visitor);
            if (result == null)
                throw BindingExceptionManager.ExpressionNodeCannotBeNull(GetType());
            return (T)result;
        }

        #endregion
    }
}