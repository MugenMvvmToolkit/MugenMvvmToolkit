#region Copyright

// ****************************************************************************
// <copyright file="IViewManager.cs">
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

using System.Threading.Tasks;
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;

namespace MugenMvvmToolkit.Interfaces
{
    /// <summary>
    ///     Represents the provider that allows to create a view for a view model
    /// </summary>
    public interface IViewManager
    {
        /// <summary>
        ///     Gets an instance of view object for the specified view model.
        /// </summary>
        /// <param name="viewModel">The view model which is now initialized.</param>
        /// <param name="context">The specified <see cref="IDataContext" />, if any.</param>
        /// <returns>
        ///     An instance of view object.
        /// </returns>
        Task<object> GetViewAsync([NotNull] IViewModel viewModel, IDataContext context = null);

        /// <summary>
        ///     Gets an instance of view object for the specified view model.
        /// </summary>
        /// <param name="viewMapping">The view mapping to create view.</param>
        /// <param name="context">The specified <see cref="IDataContext" />, if any.</param>
        /// <returns>
        ///     An instance of view object.
        /// </returns>
        Task<object> GetViewAsync([NotNull] IViewMappingItem viewMapping, IDataContext context = null);

        /// <summary>
        ///     Configures the specified view for the specified view-model.
        /// </summary>
        /// <param name="viewModel">The specified view model.</param>
        /// <param name="view">The specified view.</param>
        /// <param name="context">The specified <see cref="IDataContext" />, if any.</param>
        Task InitializeViewAsync([NotNull] IViewModel viewModel, [NotNull] object view, IDataContext context = null);

        /// <summary>
        ///     Clears view in the specified view-model
        /// </summary>
        /// <param name="viewModel">The specified view model.</param>
        /// <param name="context">The specified <see cref="IDataContext" />, if any.</param>
        Task CleanupViewAsync([NotNull] IViewModel viewModel, IDataContext context = null);
    }
}