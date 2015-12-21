#region Copyright

// ****************************************************************************
// <copyright file="XmlVisitor.cs">
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

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.Interfaces.Parse;
using MugenMvvmToolkit.Binding.Interfaces.Parse.Nodes;
using MugenMvvmToolkit.WinForms.Binding.Parse.Nodes;

namespace MugenMvvmToolkit.WinForms.Binding.Parse
{
    internal sealed class XmlVisitor : IExpressionVisitor
    {
        #region Fields

        private readonly HashSet<XmlExpressionNode> _nodes;
        private bool _isInvlalid;

        #endregion

        #region Constructors

        public XmlVisitor()
        {
            _nodes = new HashSet<XmlExpressionNode>();
        }

        #endregion

        #region Properties

        public HashSet<XmlExpressionNode> Nodes
        {
            get { return _nodes; }
        }

        public bool IsInvlalid
        {
            get { return _isInvlalid; }
        }

        public bool IsPostOrder
        {
            get { return false; }
        }

        #endregion

        #region Events

        public event Action<XmlExpressionNode> VisitNode;

        #endregion

        #region Methods

        public void Visit([NotNull] IList<XmlExpressionNode> nodes)
        {
            Should.NotBeNull(nodes, nameof(nodes));
            _nodes.Clear();
            _isInvlalid = false;
            for (int i = 0; i < nodes.Count; i++)
                nodes[i].Accept(this);
        }

        public void Raise()
        {
            Action<XmlExpressionNode> handler = VisitNode;
            if (handler == null || _nodes.Count == 0)
                return;
            foreach (var node in _nodes)
                handler(node);
        }

        public void Clear()
        {
            _nodes.Clear();
        }

        #endregion

        #region Implementation of IExpressionVisitor

        IExpressionNode IExpressionVisitor.Visit(IExpressionNode node)
        {
            var element = node as XmlExpressionNode;
            if (element != null)
            {
                _nodes.Add(element);
                if (!_isInvlalid && element is XmlInvalidExpressionNode)
                    _isInvlalid = true;
            }
            return node;
        }

        #endregion
    }
}
