#region Copyright

// ****************************************************************************
// <copyright file="XmlAttributeExpressionNode.cs">
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

using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.Interfaces.Parse;
using MugenMvvmToolkit.Binding.Interfaces.Parse.Nodes;

namespace MugenMvvmToolkit.WinForms.Binding.Parse.Nodes
{
    internal class XmlAttributeExpressionNode : XmlExpressionNode
    {
        #region Fields

        private XmlValueExpressionNode _name;
        private XmlValueExpressionNode _value;
        private XmlValueExpressionNode _equal;

        #endregion

        #region Constructors

        public XmlAttributeExpressionNode([NotNull] XmlValueExpressionNode name, XmlValueExpressionNode equal, [NotNull] XmlValueExpressionNode value, int start, int end)
            : base(start, end)
        {
            Should.NotBeNull(name, nameof(name));
            Should.NotBeNull(value, nameof(value));
            Name = name;
            Equal = equal;
            Value = value;
        }

        #endregion

        #region Properties

        [NotNull]
        public new XmlElementExpressionNode Parent => (XmlElementExpressionNode)base.Parent;

        [NotNull]
        public XmlValueExpressionNode Name
        {
            get { return _name; }
            private set { SetXmlNodeValue(ref _name, value); }
        }

        [NotNull]
        public XmlValueExpressionNode Equal
        {
            get { return _equal; }
            private set { SetXmlNodeValue(ref _equal, value); }
        }

        [NotNull]
        public XmlValueExpressionNode Value
        {
            get { return _value; }
            private set { SetXmlNodeValue(ref _value, value); }
        }

        #endregion

        #region Overrides of ExpressionNode

        protected override void AcceptInternal(IExpressionVisitor visitor)
        {
            Name = AcceptWithCheck(visitor, Name, true);
            Equal = AcceptWithCheck(visitor, Equal, true);
            Value = AcceptWithCheck(visitor, Value, true);
        }

        protected override IExpressionNode CloneInternal()
        {
            return new XmlAttributeExpressionNode((XmlValueExpressionNode)_name.Clone(), (XmlValueExpressionNode)_equal.Clone(),
                (XmlValueExpressionNode)_value.Clone(), Start, End);
        }

        #endregion
    }
}
