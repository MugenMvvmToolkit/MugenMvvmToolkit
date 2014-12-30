#region Copyright

// ****************************************************************************
// <copyright file="IMemberExpressionNode.cs">
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
    ///     Represents accessing a field or property.
    /// </summary>
    public interface IMemberExpressionNode : IExpressionNode
    {
        /// <summary>
        ///     Gets the containing object of the field or property.
        /// </summary>
        [CanBeNull]
        IExpressionNode Target { get; }

        /// <summary>
        ///     Gets the field or property to be accessed.
        /// </summary>
        [NotNull]
        string Member { get; }
    }
}