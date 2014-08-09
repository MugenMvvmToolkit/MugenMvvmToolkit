#region Copyright
// ****************************************************************************
// <copyright file="XmlVisitor.cs">
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
using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.Interfaces.Parse;
using MugenMvvmToolkit.Binding.Interfaces.Parse.Nodes;
using MugenMvvmToolkit.Binding.Parse.Nodes;

namespace MugenMvvmToolkit.Binding.Parse
{
    internal sealed class XmlVisitor : IExpressionVisitor
    {
        #region Fields

        private readonly HashSet<XmlExpressionNode> _nodes;

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

        #endregion

        #region Events

        public event Action<XmlExpressionNode> VisitNode;

        #endregion

        #region Methods

        public void Visit([NotNull] IList<XmlExpressionNode> nodes)
        {
            Should.NotBeNull(nodes, "nodes");
            _nodes.Clear();
            for (int i = 0; i < nodes.Count; i++)
                nodes[i].Accept(this);
        }

        #endregion

        #region Implementation of IExpressionVisitor

        IExpressionNode IExpressionVisitor.Visit(IExpressionNode node)
        {
            var element = node as XmlExpressionNode;
            if (element != null)
            {
                if (_nodes.Add(element))
                {
                    Action<XmlExpressionNode> handler = VisitNode;
                    if (handler != null)
                        handler.Invoke(element);
                }
            }
            return node;
        }

        #endregion
    }
}