#region Copyright

// ****************************************************************************
// <copyright file="ILambdaExpressionNode.cs">
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

using System.Collections.Generic;
using JetBrains.Annotations;

namespace MugenMvvmToolkit.Binding.Interfaces.Parse.Nodes
{
    /// <summary>
    ///     Represents an expression that has a lambda operator.
    /// </summary>
    public interface ILambdaExpressionNode : IExpressionNode
    {
        /// <summary>
        ///     Gets the parameters of lambda expression.
        /// </summary>
        [NotNull]
        IList<string> Parameters { get; }

        /// <summary>
        ///     Gets the lambda expression.
        /// </summary>
        [NotNull]
        IExpressionNode Expression { get; }
    }
}