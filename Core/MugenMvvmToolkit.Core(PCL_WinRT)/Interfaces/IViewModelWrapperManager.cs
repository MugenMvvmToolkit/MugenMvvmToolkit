#region Copyright
// ****************************************************************************
// <copyright file="IViewModelWrapperManager.cs">
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
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;

namespace MugenMvvmToolkit.Interfaces
{
    /// <summary>
    ///     Represents the wrapper manager that allows to wrap a view model to another view model.
    /// </summary>
    public interface IViewModelWrapperManager
    {
        /// <summary>
        ///     Wraps the specified view-model to a specified type.
        /// </summary>
        /// <param name="viewModel">The specified view-model.</param>
        /// <param name="wrapperType">The specified type to wrap.</param>
        /// <param name="dataContext">The specified <see cref="IDataContext" />, if any.</param>
        /// <returns>
        ///     An instance of <see cref="IViewModel" />.
        /// </returns>
        [NotNull]
        IViewModel Wrap([NotNull] IViewModel viewModel, [NotNull] Type wrapperType, [CanBeNull] IDataContext dataContext);
    }
}