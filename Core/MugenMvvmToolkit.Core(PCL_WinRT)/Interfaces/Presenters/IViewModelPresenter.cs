#region Copyright

// ****************************************************************************
// <copyright file="IViewModelPresenter.cs">
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

using System.Collections.Generic;
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces.Callbacks;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;

namespace MugenMvvmToolkit.Interfaces.Presenters
{
    /// <summary>
    ///     Represents the service that allows to show a view model.
    /// </summary>
    public interface IViewModelPresenter
    {
        /// <summary>
        ///     Gets the collection of <see cref="IDynamicViewModelPresenter" />.
        /// </summary>
        [NotNull]
        ICollection<IDynamicViewModelPresenter> DynamicPresenters { get; }

        /// <summary>
        ///     Shows the specified <see cref="IViewModel" />.
        /// </summary>
        /// <param name="viewModel">The specified <see cref="IViewModel" /> to show.</param>
        /// <param name="context">The specified context.</param>
        [NotNull]
        IAsyncOperation<bool?> ShowAsync([NotNull] IViewModel viewModel, [CanBeNull] IDataContext context);

        /// <summary>
        ///     Tries to restore the presenter state of the specified <see cref="IViewModel" />.
        /// </summary>
        /// <param name="viewModel">The specified <see cref="IViewModel" /> to show.</param>
        /// <param name="context">The specified context.</param>
        void Restore([NotNull] IViewModel viewModel, [CanBeNull] IDataContext context);
    }
}