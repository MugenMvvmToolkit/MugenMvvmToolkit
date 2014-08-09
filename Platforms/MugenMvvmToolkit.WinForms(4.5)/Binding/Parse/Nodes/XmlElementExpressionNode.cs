#region Copyright
// ****************************************************************************
// <copyright file="XmlElementExpressionNode.cs">
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
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.Interfaces.Parse;
using MugenMvvmToolkit.Binding.Interfaces.Parse.Nodes;

namespace MugenMvvmToolkit.Binding.Parse.Nodes
{
    internal class XmlElementExpressionNode : XmlExpressionNode
    {
        #region Fields

        private readonly List<XmlExpressionNode> _attributes;
        private readonly List<XmlExpressionNode> _elements;
        private readonly string _name;
        private XmlValueExpressionNode _endTag;
        private XmlValueExpressionNode _startTag;
        private XmlValueExpressionNode _startTagEnd;

        #endregion

        #region Constructors

        public XmlElementExpressionNode([NotNull]XmlValueExpressionNode startTag, string name, int start, int end)
            : base(start, end)
        {
            Should.NotBeNull(startTag, "startTag");
            Should.NotBeNull(name, "name");
            StartTag = startTag;
            _name = name;
            _elements = new List<XmlExpressionNode>();
            _attributes = new List<XmlExpressionNode>();
        }

        #endregion

        #region Properties

        public bool IsComplex
        {
            get { return _startTagEnd != null; }
        }

        public bool IsValid
        {
            get { return EndTag != null; }
        }

        [NotNull]
        public string Name
        {
            get { return _name; }
        }

        [CanBeNull]
        public new XmlElementExpressionNode Parent
        {
            get { return (XmlElementExpressionNode)base.Parent; }
        }

        [NotNull]
        public IEnumerable<XmlExpressionNode> Elements
        {
            get { return _elements; }
        }

        [NotNull]
        public IEnumerable<XmlExpressionNode> Attributes
        {
            get { return _attributes; }
        }

        [NotNull]
        public XmlValueExpressionNode StartTag
        {
            get { return _startTag; }
            private set { SetXmlNodeValue(ref _startTag, value); }
        }

        [CanBeNull]
        public XmlValueExpressionNode StartTagEnd
        {
            get { return _startTagEnd; }
            private set { SetXmlNodeValue(ref _startTagEnd, value); }
        }

        [CanBeNull]
        public XmlValueExpressionNode EndTag
        {
            get { return _endTag; }
            private set { SetXmlNodeValue(ref _endTag, value); }
        }

        #endregion

        #region Methods

        public void UpdateStartTagEnd([NotNull]XmlValueExpressionNode startTagEnd)
        {
            Should.NotBeNull(startTagEnd, "startTagEnd");
            StartTagEnd = startTagEnd;
        }

        public void UpdateCloseTag([NotNull]XmlValueExpressionNode endTag, int endPosition)
        {
            Should.NotBeNull(endTag, "endTag");
            EndTag = endTag;
            UpdatePosition(Start, endPosition);
        }

        public void AddElement([NotNull] XmlExpressionNode node)
        {
            Should.NotBeNull(node, "node");
            if (_elements.Contains(node))
                return;
            _elements.Add(node);
            UpdateParent(node, this);
        }

        public bool RemoveElement([NotNull]XmlExpressionNode node)
        {
            Should.NotBeNull(node, "node");
            if (_elements.Remove(node))
            {
                UpdateParent(node, null);
                return true;
            }
            return false;
        }

        public void ClearElements()
        {
            for (int i = 0; i < _elements.Count; i++)
                UpdateParent(_elements[i], null);
            _elements.Clear();
        }

        public void AddAttribute([NotNull] XmlExpressionNode node)
        {
            Should.NotBeNull(node, "node");
            if (_attributes.Contains(node))
                return;
            _attributes.Add(node);
            UpdateParent(node, this);
        }

        public bool RemoveAttribute([NotNull] XmlExpressionNode node)
        {
            Should.NotBeNull(node, "node");
            if (_attributes.Remove(node))
            {
                UpdateParent(node, null);
                return true;
            }
            return false;
        }

        public void ClearAttributes()
        {
            for (int i = 0; i < _attributes.Count; i++)
                UpdateParent(_attributes[i], null);
            _attributes.Clear();
        }

        #endregion

        #region Overrides of ExpressionNode

        protected override void AcceptInternal(IExpressionVisitor visitor)
        {
            StartTag = AcceptWithCheck(visitor, StartTag, true);
            if (StartTagEnd != null)
                StartTagEnd = AcceptWithCheck(visitor, StartTagEnd, true);
            if (EndTag != null)
                EndTag = AcceptWithCheck(visitor, EndTag, true);
            for (int i = 0; i < _elements.Count; i++)
            {
                XmlExpressionNode node = AcceptWithCheck(visitor, _elements[i], false);
                if (node == null)
                {
                    _elements.RemoveAt(i);
                    i--;
                    continue;
                }
                _elements[i] = node;
            }

            for (int i = 0; i < _attributes.Count; i++)
            {
                XmlExpressionNode node = AcceptWithCheck(visitor, _attributes[i], false);
                if (node == null)
                {
                    _attributes.RemoveAt(i);
                    i--;
                    continue;
                }
                _attributes[i] = node;
            }
        }

        protected override IExpressionNode CloneInternal()
        {
            var node = new XmlElementExpressionNode((XmlValueExpressionNode)_startTag.Clone(), _name, Start, End)
            {
                EndTag = (XmlValueExpressionNode)_endTag.Clone(),
                StartTagEnd = (XmlValueExpressionNode)_startTagEnd.Clone()
            };
            foreach (XmlExpressionNode element in _elements)
                node.AddElement((XmlExpressionNode)element.Clone());

            foreach (XmlExpressionNode element in _attributes)
                node.AddAttribute((XmlAttributeExpressionNode)element.Clone());
            return node;
        }

        #endregion
    }
}