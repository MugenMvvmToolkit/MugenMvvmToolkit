#region Copyright

// ****************************************************************************
// <copyright file="XmlCommentExpressionNode.cs">
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
    internal class XmlCommentExpressionNode : XmlExpressionNode
    {
        #region Constructors

        public XmlCommentExpressionNode(int start, int end)
            : base(start, end)
        {
        }

        #endregion

        #region Overrides of ExpressionNode

        protected override void AcceptInternal(IExpressionVisitor visitor)
        {
        }

        protected override IExpressionNode CloneInternal()
        {
            return new XmlCommentExpressionNode(Start, End);
        }

        #endregion
    }
}
