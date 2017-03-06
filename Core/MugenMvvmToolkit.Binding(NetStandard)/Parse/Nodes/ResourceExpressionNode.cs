#region Copyright

// ****************************************************************************
// <copyright file="ResourceExpressionNode.cs">
// Copyright (c) 2012-2017 Vyacheslav Volkov
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
using MugenMvvmToolkit.Binding.Models;

namespace MugenMvvmToolkit.Binding.Parse.Nodes
{
    public sealed class ResourceExpressionNode : ExpressionNode
    {
        #region Fields

        public static readonly ResourceExpressionNode StaticInstance;
        public static readonly ResourceExpressionNode DynamicInstance;

        private readonly string _name;
        private readonly bool _dynamic;

        #endregion

        #region Constructors

        static ResourceExpressionNode()
        {
            StaticInstance = new ResourceExpressionNode("$$", false);
            DynamicInstance = new ResourceExpressionNode("$", true);
        }

        private ResourceExpressionNode(string name, bool @dynamic)
            : base(ExpressionNodeType.DynamicMember)
        {
            _name = name;
            _dynamic = dynamic;
        }

        #endregion

        #region Properties

        public bool Dynamic => _dynamic;

        #endregion

        #region Overrides of ExpressionNode

        protected override void AcceptInternal(IExpressionVisitor visitor)
        {
        }

        protected override IExpressionNode CloneInternal()
        {
            return this;
        }

        #endregion

        #region Overrides of Object

        public override string ToString()
        {
            return _name;
        }

        #endregion
    }
}
