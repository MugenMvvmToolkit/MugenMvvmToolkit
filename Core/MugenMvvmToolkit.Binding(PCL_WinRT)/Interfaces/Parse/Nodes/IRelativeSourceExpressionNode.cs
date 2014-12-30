#region Copyright

// ****************************************************************************
// <copyright file="IRelativeSourceExpressionNode.cs">
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
    ///     Represents accessing a relative source.
    /// </summary>
    public interface IRelativeSourceExpressionNode : IExpressionNode
    {
        /// <summary>
        ///     Gets the type of relative source.
        /// </summary>
        [NotNull]
        string Type { get; }

        /// <summary>
        ///     Gets the element name, if any.
        /// </summary>
        [CanBeNull]
        string ElementName { get; }

        /// <summary>
        ///     Gets the path, if any.
        /// </summary>
        [CanBeNull]
        string Path { get; }

        /// <summary>
        ///     Gets the level.
        /// </summary>
        uint Level { get; }

        /// <summary>
        /// Merges the current path with specified.
        /// </summary>        
        void MergePath(string path);
    }
}