#region Copyright

// ****************************************************************************
// <copyright file="IViewMappingItem.cs">
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

namespace MugenMvvmToolkit.Interfaces.Models
{
    /// <summary>
    ///     Represents the interface that contains information about a mapping from view to a view model.
    /// </summary>
    public interface IViewMappingItem
    {
        /// <summary>
        ///     Gets the name of mapping.
        /// </summary>
        [CanBeNull]
        string Name { get; }

        /// <summary>
        ///     Gets the type of view.
        /// </summary>
        [NotNull]
        Type ViewType { get; }

        /// <summary>
        ///     Gets or sets the type of view model.
        /// </summary>
        [NotNull]
        Type ViewModelType { get; }

        /// <summary>
        ///     Gets the uri, if any.
        /// </summary>
        [NotNull]
        Uri Uri { get; }
    }
}