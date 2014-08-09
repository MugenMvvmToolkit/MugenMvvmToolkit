#region Copyright
// ****************************************************************************
// <copyright file="IIndexExpressionNode.cs">
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
    ///     Represents indexing a property or array.
    /// </summary>
    public interface IIndexExpressionNode : IExpressionNode
    {
        /// <summary>
        ///     An object to index.
        /// </summary>
        [NotNull]
        IExpressionNode Object { get; }

        /// <summary>
        ///     Gets the arguments that will be used to index the property or array.
        /// </summary>
        [NotNull]
        IList<IExpressionNode> Arguments { get; }
    }
}