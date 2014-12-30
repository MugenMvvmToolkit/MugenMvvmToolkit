#region Copyright

// ****************************************************************************
// <copyright file="XmlInvalidExpressionNode.cs">
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

namespace MugenMvvmToolkit.Binding.Parse.Nodes
{
    internal enum XmlInvalidExpressionType
    {
        Unknown = 0,
        Attribute = 1,
        Element = 2,
        ElementValue = 3,
        Comment = 4
    }

    internal class XmlInvalidExpressionNode : XmlExpressionNode
    {
        #region Fields

        private readonly IList<XmlExpressionNode> _nodes;
        private readonly XmlInvalidExpressionType _type;

        #endregion

        #region Constructors

        public XmlInvalidExpressionNode(XmlInvalidExpressionType type, int start, int end)
            : base(start, end)
        {
            _type = type;
        }

        public XmlInvalidExpressionNode(IList<XmlExpressionNode> nodes, XmlInvalidExpressionType type, int start, int end)
            : this(type, start, end)
        {
            _nodes = nodes;
        }

        #endregion

        #region Properties

        public XmlInvalidExpressionType Type
        {
            get { return _type; }
        }

        public IList<XmlExpressionNode> Nodes
        {
            get { return _nodes; }
        }

        #endregion

        #region Overrides of ExpressionNode

        protected override void AcceptInternal(IExpressionVisitor visitor)
        {
            if (_nodes == null)
                return;
            for (int index = 0; index < _nodes.Count; index++)
                _nodes[index] = AcceptWithCheck(visitor, _nodes[index], true);
        }

        protected override IExpressionNode CloneInternal()
        {
            return new XmlInvalidExpressionNode(Type, Start, End);
        }

        #endregion
    }
}