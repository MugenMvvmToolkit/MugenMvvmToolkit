#region Copyright

// ****************************************************************************
// <copyright file="IConstantExpressionNode.cs">
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

using System;
using JetBrains.Annotations;

namespace MugenMvvmToolkit.Binding.Interfaces.Parse.Nodes
{
    /// <summary>
    ///     Represents an expression that has a constant value.
    /// </summary>
    public interface IConstantExpressionNode : IExpressionNode
    {
        /// <summary>
        ///     Gets the value of the constant expression.
        /// </summary>
        [CanBeNull]
        object Value { get; }

        /// <summary>
        ///     Gets the type of the value.
        /// </summary>
        [NotNull]
        Type Type { get; }
    }
}