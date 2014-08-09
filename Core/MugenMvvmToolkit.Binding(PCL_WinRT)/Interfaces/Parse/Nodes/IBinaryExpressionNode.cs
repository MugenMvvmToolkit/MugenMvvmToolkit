#region Copyright
// ****************************************************************************
// <copyright file="IBinaryExpressionNode.cs">
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
    ///     Represents an expression that has a binary operator.
    /// </summary>
    public interface IBinaryExpressionNode : IExpressionNode
    {
        /// <summary>
        ///     Gets the left operand of the binary operation.
        /// </summary>
        [NotNull]
        IExpressionNode Left { get; }

        /// <summary>
        ///     Gets the right operand of the binary operation.
        /// </summary>
        [NotNull]
        IExpressionNode Right { get; }

        /// <summary>
        ///     Gets the implementing token for the binary operation.
        /// </summary>
        TokenType Token { get; }
    }
}