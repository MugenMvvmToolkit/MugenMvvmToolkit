#region Copyright

// ****************************************************************************
// <copyright file="IndexExpressionNode.cs">
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

using System.Collections.Generic;
using MugenMvvmToolkit.Binding.Interfaces.Parse;
using MugenMvvmToolkit.Binding.Interfaces.Parse.Nodes;
using MugenMvvmToolkit.Binding.Models;

namespace MugenMvvmToolkit.Binding.Parse.Nodes
{
    public class IndexExpressionNode : ExpressionNode, IIndexExpressionNode
    {
        #region Fields

        private readonly IList<IExpressionNode> _arguments;
        private IExpressionNode _object;

        #endregion

        #region Constructors

        public IndexExpressionNode(IExpressionNode obj, IList<IExpressionNode> args)
            : base(ExpressionNodeType.Index)
        {
            _arguments = args == null ? Empty.Array<IExpressionNode>() : args.ToArrayEx();
            _object = obj;
        }

        #endregion

        #region Implementation of IIndexExpressionNode

        public IExpressionNode Object
        {
            get { return _object; }
        }

        public IList<IExpressionNode> Arguments
        {
            get { return _arguments; }
        }

        #endregion

        #region Overrides of ExpressionNode

        protected override void AcceptInternal(IExpressionVisitor visitor)
        {
            if (_object != null)
                _object = AcceptWithCheck(visitor, _object, false);
            for (int index = 0; index < Arguments.Count; index++)
            {
                _arguments[index] = AcceptWithCheck(visitor, _arguments[index], true);
            }
        }

        protected override IExpressionNode CloneInternal()
        {
            return new IndexExpressionNode(_object == null ? null : Object.Clone(), _arguments.ToArrayEx(node => node.Clone()));
        }

        public override string ToString()
        {
            string @join = string.Join(", ", Arguments);
            return $"{Object}[{@join}]";
        }

        #endregion
    }
}
