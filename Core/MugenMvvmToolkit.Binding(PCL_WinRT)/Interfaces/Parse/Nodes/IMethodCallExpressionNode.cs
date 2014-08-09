#region Copyright
// ****************************************************************************
// <copyright file="IMethodCallExpressionNode.cs">
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
using System.Collections.Generic;
using JetBrains.Annotations;

namespace MugenMvvmToolkit.Binding.Interfaces.Parse.Nodes
{
    /// <summary>
    ///     Represents a call to either static or an instance method.
    /// </summary>
    public interface IMethodCallExpressionNode : IExpressionNode
    {
        /// <summary>
        ///     Gets the type arguments for the method.
        /// </summary>
        [NotNull]
        IList<string> TypeArgs { get; }

        /// <summary>
        ///     Gets the method name for the method to be called.
        /// </summary>
        [NotNull]
        string Method { get; }

        /// <summary>
        ///     Gets the expression that represents the instance for instance method calls or null for static method calls.
        /// </summary>
        [CanBeNull]
        IExpressionNode Target { get; }

        /// <summary>
        ///     Gets a collection of expressions that represent arguments of the called method.
        /// </summary>
        [NotNull]
        IList<IExpressionNode> Arguments { get; }        
    }
}