#region Copyright
// ****************************************************************************
// <copyright file="IndexExpressionNode.cs">
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
using System.Collections.Generic;
using MugenMvvmToolkit.Binding.Interfaces.Parse;
using MugenMvvmToolkit.Binding.Interfaces.Parse.Nodes;
using MugenMvvmToolkit.Binding.Models;

namespace MugenMvvmToolkit.Binding.Parse.Nodes
{
    /// <summary>
    ///     Represents indexing a property or array.
    /// </summary>
    public class IndexExpressionNode : ExpressionNode, IIndexExpressionNode
    {
        #region Fields

        private readonly IList<IExpressionNode> _arguments;
        private IExpressionNode _object;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="IndexExpressionNode" /> class.
        /// </summary>
        public IndexExpressionNode(IExpressionNode obj, IList<IExpressionNode> args)
            : base(ExpressionNodeType.Index)
        {
            _arguments = args == null ? Empty.Array<IExpressionNode>() : args.ToArrayEx();
            _object = obj;
        }

        #endregion

        #region Implementation of IIndexExpressionNode

        /// <summary>
        ///     An object to index.
        /// </summary>
        public IExpressionNode Object
        {
            get { return _object; }
        }

        /// <summary>
        ///     Gets the arguments that will be used to index the property or array.
        /// </summary>
        public IList<IExpressionNode> Arguments
        {
            get { return _arguments; }
        }

        #endregion

        #region Overrides of ExpressionNode

        /// <summary>
        ///     Calls the visitor on the expression.
        /// </summary>
        protected override void AcceptInternal(IExpressionVisitor visitor)
        {
            if (_object != null)
                _object = AcceptWithCheck(visitor, _object, false);
            for (int index = 0; index < Arguments.Count; index++)
            {
                _arguments[index] = AcceptWithCheck(visitor, _arguments[index], true);
            }
        }

        /// <summary>
        ///     Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        ///     A new object that is a copy of this instance.
        /// </returns>
        protected override IExpressionNode CloneInternal()
        {
            return new IndexExpressionNode(_object == null ? null : Object.Clone(), _arguments.ToArrayEx(node => node.Clone()));
        }

        /// <summary>
        ///     Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        ///     A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            string @join = string.Join(", ", Arguments);
            return string.Format("{0}[{1}]", Object, join);
        }

        #endregion
    }
}