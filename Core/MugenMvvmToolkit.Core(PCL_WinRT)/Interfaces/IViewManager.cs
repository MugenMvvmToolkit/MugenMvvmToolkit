#region Copyright
// ****************************************************************************
// <copyright file="IViewManager.cs">
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
using System.Threading.Tasks;
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Interfaces.Views;

namespace MugenMvvmToolkit.Interfaces
{
    /// <summary>
    ///     Represents the provider which creates a view for a view model
    /// </summary>
    public interface IViewManager
    {
        /// <summary>
        ///     Gets the type of view wrapper.
        /// </summary>
        Type GetViewType([NotNull] Type viewType, [CanBeNull]IDataContext dataContext);

        /// <summary>
        ///     Wraps the specified view object to a <see cref="IView" />.
        /// </summary>
        IView WrapToView([NotNull] object view, [CanBeNull] IDataContext dataContext);

        /// <summary>
        ///     Gets an instance of <see cref="IView" /> for the specified view model.
        /// </summary>
        /// <param name="viewModel">The view model which is now initialized.</param>
        /// <param name="dataContext">The specified <see cref="IDataContext" />.</param>
        /// <returns>
        ///     An instance of <see cref="IView" />.
        /// </returns>
        Task<IView> GetViewAsync([NotNull] IViewModel viewModel, [CanBeNull] IDataContext dataContext);

        /// <summary>
        ///     Configures the specified view for the specified view-model.
        /// </summary>
        /// <param name="viewModel">The specified view model.</param>
        /// <param name="view">The specified view.</param>
        Task InitializeViewAsync([NotNull] IViewModel viewModel, [NotNull] object view);

        /// <summary>
        ///     Clears view in the specified view-model
        /// </summary>
        /// <param name="viewModel">The specified view model.</param>
        Task CleanupViewAsync([NotNull] IViewModel viewModel);
    }
}