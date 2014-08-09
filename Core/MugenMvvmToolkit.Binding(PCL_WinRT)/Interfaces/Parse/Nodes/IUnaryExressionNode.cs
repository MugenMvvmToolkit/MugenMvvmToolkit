#region Copyright
// ****************************************************************************
// <copyright file="IUnaryExressionNode.cs">
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
    ///     Represents an expression that has a unary operator.
    /// </summary>
    public interface IUnaryExressionNode : IExpressionNode
    {
        /// <summary>
        ///     Gets the token of the unary operation.
        /// </summary>
        TokenType Token { get; }

        /// <summary>
        ///     Gets the operand of the unary operation.
        /// </summary>
        [NotNull]
        IExpressionNode Operand { get; }
    }
}