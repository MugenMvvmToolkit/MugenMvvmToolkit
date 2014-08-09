#region Copyright
// ****************************************************************************
// <copyright file="IExpressionNode.cs">
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
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.Models;

namespace MugenMvvmToolkit.Binding.Interfaces.Parse.Nodes
{
    /// <summary>
    ///     Provides the base interface from which the classes that represent expression tree nodes are derived.
    /// </summary>
    public interface IExpressionNode
    {
        /// <summary>
        ///     Gets the node type of this <see cref="IExpressionNode" />.
        /// </summary>
        [NotNull]
        ExpressionNodeType NodeType { get; }

        /// <summary>
        ///     Dispatches to the specific visit method for this node type.
        /// </summary>
        /// <param name="visitor">The visitor to visit this node with.</param>
        /// <returns>
        ///     The result of visiting this node.
        /// </returns>
        IExpressionNode Accept([NotNull] IExpressionVisitor visitor);

        /// <summary>
        ///     Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        ///     A new object that is a copy of this instance.
        /// </returns>
        [NotNull]
        IExpressionNode Clone();
    }
}