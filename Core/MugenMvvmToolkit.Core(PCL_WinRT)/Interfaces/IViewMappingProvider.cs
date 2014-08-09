#region Copyright
// ****************************************************************************
// <copyright file="IViewMappingProvider.cs">
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
using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Interfaces
{
    /// <summary>
    ///     Represents the interface to provide view mappings.
    /// </summary>
    public interface IViewMappingProvider
    {
        /// <summary>
        ///     Gets the series instances of <see cref="IViewMappingItem" />.
        /// </summary>
        [NotNull]
        IEnumerable<IViewMappingItem> ViewMappings { get; }

        /// <summary>
        ///     Finds the series of <see cref="IViewMappingItem" /> for the specified type of view.
        /// </summary>
        /// <param name="viewType">The specified type of view.</param>
        /// <param name="throwOnError">
        ///     true to throw an exception if the type cannot be found; false to return null. Specifying
        ///     false also suppresses some other exception conditions, but not all of them.
        /// </param>
        /// <returns>
        ///     The series of <see cref="IViewMappingItem" />.
        /// </returns>
        [NotNull]
        IList<IViewMappingItem> FindMappingsForView([NotNull] Type viewType, bool throwOnError);

        /// <summary>
        ///     Finds the <see cref="IViewMappingItem" /> for the specified type of view model.
        /// </summary>
        /// <param name="viewModelType">The specified type of view model.</param>
        /// <param name="viewName">The specified name of view, if any.</param>
        /// <param name="throwOnError">
        ///     true to throw an exception if the type cannot be found; false to return null. Specifying
        ///     false also suppresses some other exception conditions, but not all of them.
        /// </param>
        /// <returns>
        ///     An instance of <see cref="IViewMappingItem" />.
        /// </returns>
        IViewMappingItem FindMappingForViewModel([NotNull] Type viewModelType, [CanBeNull] string viewName,
            bool throwOnError);
    }
}