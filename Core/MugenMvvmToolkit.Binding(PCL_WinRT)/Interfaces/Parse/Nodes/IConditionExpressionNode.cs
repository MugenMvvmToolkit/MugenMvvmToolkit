#region Copyright

// ****************************************************************************
// <copyright file="IConditionExpressionNode.cs">
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

using JetBrains.Annotations;

namespace MugenMvvmToolkit.Binding.Interfaces.Parse.Nodes
{
    /// <summary>
    ///     Represents an expression that has a conditional operator.
    /// </summary>
    public interface IConditionExpressionNode : IExpressionNode
    {
        /// <summary>
        ///     Gets the test of the conditional operation.
        /// </summary>
        [NotNull]
        IExpressionNode Condition { get; }

        /// <summary>
        ///     Gets the expression to execute if the test evaluates to true.
        /// </summary>
        [NotNull]
        IExpressionNode IfTrue { get; }

        /// <summary>
        ///     Gets the expression to execute if the test evaluates to false.
        /// </summary>
        [NotNull]
        IExpressionNode IfFalse { get; }
    }
}