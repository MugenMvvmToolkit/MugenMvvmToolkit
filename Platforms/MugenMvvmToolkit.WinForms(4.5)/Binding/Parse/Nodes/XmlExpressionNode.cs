#region Copyright

// ****************************************************************************
// <copyright file="XmlExpressionNode.cs">
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

using MugenMvvmToolkit.Binding.Models;

namespace MugenMvvmToolkit.Binding.Parse.Nodes
{
    internal abstract class XmlExpressionNode : ExpressionNode
    {
        #region Fields

        private static readonly ExpressionNodeType XmlType;
        private int _end;
        private int _length;
        private XmlExpressionNode _parent;
        private int _start;

        #endregion

        #region Constructors

        static XmlExpressionNode()
        {
            XmlType = new ExpressionNodeType("xml");
        }

        protected XmlExpressionNode(int start, int end)
            : base(XmlType)
        {
            UpdatePosition(start, end);
        }

        #endregion

        #region Properties

        public XmlExpressionNode Parent
        {
            get { return _parent; }
            private set
            {
                _parent = value;
                OnParentChanged();
            }
        }

        public int Start
        {
            get { return _start; }
        }

        public int End
        {
            get { return _end; }
        }

        public int Length
        {
            get { return _length; }
        }

        #endregion

        #region Methods

        public void UpdatePosition(int start, int end)
        {
            _start = start;
            _end = end;
            _length = end - start;
        }

        public string GetValue(string source)
        {
            return source.Substring(_start, _length);
        }

        protected void SetXmlNodeValue<TNode>(ref TNode node, TNode value)
            where TNode : XmlExpressionNode
        {
            if (ReferenceEquals(node, value))
                return;
            if (node != null)
                node.Parent = null;
            node = value;
            if (node != null)
                node.Parent = this;
        }

        protected static void UpdateParent(XmlExpressionNode node, XmlExpressionNode parent)
        {
            if (ReferenceEquals(node.Parent, parent))
                return;
            node.Parent = parent;
        }

        protected virtual void OnParentChanged()
        {
        }

        #endregion
    }
}