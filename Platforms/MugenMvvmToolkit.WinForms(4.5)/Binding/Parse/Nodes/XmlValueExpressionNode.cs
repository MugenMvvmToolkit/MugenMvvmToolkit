#region Copyright

// ****************************************************************************
// <copyright file="XmlValueExpressionNode.cs">
// Copyright (c) 2012-2016 Vyacheslav Volkov
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

using MugenMvvmToolkit.Binding.Interfaces.Parse;
using MugenMvvmToolkit.Binding.Interfaces.Parse.Nodes;

namespace MugenMvvmToolkit.WinForms.Binding.Parse.Nodes
{
    internal enum XmlValueExpressionType
    {
        ElementStartTag = 1,
        ElementStartTagEnd = 2,
        ElementEndTag = 3,
        ElementValue = 4,
        AttributeName = 5,
        AttributeEqual = 6,
        AttributeValue = 7
    }

    internal class XmlValueExpressionNode : XmlExpressionNode
    {
        #region Fields

        private readonly XmlValueExpressionType _type;

        #endregion

        #region Constructors

        public XmlValueExpressionNode(XmlExpressionNode parent, XmlValueExpressionType type, int start, int end)
            : this(type, start, end)
        {
            UpdateParent(this, parent);
        }

        public XmlValueExpressionNode(XmlValueExpressionType type, int start, int end)
            : base(start, end)
        {
            _type = type;
        }

        #endregion

        public XmlValueExpressionType Type => _type;

        #region Overrides of ExpressionNode

        protected override void AcceptInternal(IExpressionVisitor visitor)
        {
        }

        protected override IExpressionNode CloneInternal()
        {
            return new XmlValueExpressionNode(_type, Start, End);
        }

        #endregion
    }
}
