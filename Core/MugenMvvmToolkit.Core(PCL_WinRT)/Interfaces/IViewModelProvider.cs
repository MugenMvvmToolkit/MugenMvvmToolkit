#region Copyright
// ****************************************************************************
// <copyright file="IViewModelProvider.cs">
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
using JetBrains.Annotations;
using MugenMvvmToolkit.Annotations;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Interfaces
{
    /// <summary>
    ///     Represents the provider that allows to creates and restores the view models.
    /// </summary>
    public interface IViewModelProvider
    {
        /// <summary>
        ///     Creates an instance of the specified view model.
        /// </summary>
        /// <param name="getViewModel">The specified delegate to create view model.</param>
        /// <param name="dataContext">The specified <see cref="IDataContext" />.</param>
        /// <returns>
        ///     An instance of <see cref="IViewModel" />.
        /// </returns>
        [Pure, NotNull]
        IViewModel GetViewModel([NotNull] GetViewModelDelegate<IViewModel> getViewModel,
            [NotNull] IDataContext dataContext);

        /// <summary>
        ///     Creates an instance of the specified view model.
        /// </summary>
        /// <param name="viewModelType">The type of view model.</param>
        /// <param name="dataContext">The specified <see cref="IDataContext" />.</param>
        /// <returns>
        ///     An instance of <see cref="IViewModel" />.
        /// </returns>
        [Pure, NotNull]
        IViewModel GetViewModel([NotNull, ViewModelTypeRequired] Type viewModelType, [NotNull] IDataContext dataContext);

        /// <summary>
        ///     Initializes the specified <see cref="IViewModel" />, use this method if you have created an
        ///     <see cref="IViewModel" />
        ///     without using the GetViewModel method.
        /// </summary>
        /// <param name="viewModel">
        ///     The specified <see cref="IViewModel" />.
        /// </param>
        /// <param name="dataContext">The specified <see cref="IDataContext" />.</param>
        void InitializeViewModel([NotNull] IViewModel viewModel, [CanBeNull] IDataContext dataContext);

        /// <summary>
        ///     Preserves the view model state.
        /// </summary>
        [NotNull, Pure]
        IDataContext PreserveViewModel([NotNull] IViewModel viewModel, [CanBeNull] IDataContext dataContext);

        /// <summary>
        ///     Restores the view model from state context.
        /// </summary>
        /// <param name="viewModelState">The specified state <see cref="IDataContext" />.</param>
        /// <param name="throwOnError">
        ///     <c>true</c> to throw an exception if the view model cannot be restored; <c>false</c> to return null.
        /// </param>
        /// <param name="dataContext">The specified <see cref="IDataContext" />.</param>
        /// <returns>
        ///     An instance of <see cref="IViewModel" />.
        /// </returns>
        [Pure]
        IViewModel RestoreViewModel([CanBeNull] IDataContext viewModelState, [CanBeNull] IDataContext dataContext, bool throwOnError);
    }
}