#region Copyright
// ****************************************************************************
// <copyright file="ResourceExpressionNode.cs">
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
using MugenMvvmToolkit.Binding.Interfaces.Parse;
using MugenMvvmToolkit.Binding.Interfaces.Parse.Nodes;
using MugenMvvmToolkit.Binding.Models;

namespace MugenMvvmToolkit.Binding.Parse.Nodes
{
    public sealed class ResourceExpressionNode : ExpressionNode
    {
        #region Fields

        public static readonly ResourceExpressionNode StaticInstance = new ResourceExpressionNode("$$", false);

        public static readonly ResourceExpressionNode DynamicInstance = new ResourceExpressionNode("$", true);

        private readonly string _name;
        private readonly bool _dynamic;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ResourceExpressionNode" /> class.
        /// </summary>
        private ResourceExpressionNode(string name, bool @dynamic)
            : base(ExpressionNodeType.DynamicMember)
        {
            _name = name;
            _dynamic = dynamic;
        }

        #endregion

        #region Properties

        public bool Dynamic
        {
            get { return _dynamic; }            
        }

        #endregion

        #region Overrides of ExpressionNode

        /// <summary>
        ///     Dispatches to the specific visit method for this node type.
        /// </summary>
        /// <param name="visitor">The visitor to visit this node with.</param>
        protected override void AcceptInternal(IExpressionVisitor visitor)
        {
        }

        /// <summary>
        ///     Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        ///     A new object that is a copy of this instance.
        /// </returns>
        protected override IExpressionNode CloneInternal()
        {
            return this;
        }

        #endregion

        #region Overrides of Object

        /// <summary>
        ///     Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        ///     A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            return _name;
        }

        #endregion
    }
}