#region Copyright

// ****************************************************************************
// <copyright file="IDynamicViewModelPresenter.cs">
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
using MugenMvvmToolkit.Interfaces.Callbacks;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;

namespace MugenMvvmToolkit.Interfaces.Presenters
{
    /// <summary>
    ///     Represents the service that allows to show a view model.
    /// </summary>
    public interface IDynamicViewModelPresenter
    {
        /// <summary>
        ///     Gets the presenter priority.
        /// </summary>
        int Priority { get; }

        /// <summary>
        ///     Tries to show the specified <see cref="IViewModel" />.
        /// </summary>
        /// <param name="viewModel">The specified <see cref="IViewModel" /> to show.</param>
        /// <param name="context">The specified context.</param>
        /// <param name="parentPresenter">The parent presenter, if any.</param>
        [CanBeNull]
        IAsyncOperation<bool?> TryShowAsync([NotNull] IViewModel viewModel, [NotNull] IDataContext context,
            [CanBeNull] IViewModelPresenter parentPresenter);
    }

    /// <summary>
    ///     Represents the service that allows to show a view model and restore view model presenter state.
    /// </summary>
    public interface IRestorableDynamicViewModelPresenter : IDynamicViewModelPresenter
    {
        /// <summary>
        /// Tries to restore the presenter state of the specified <see cref="IViewModel" />.
        /// </summary>
        /// <param name="viewModel">The specified <see cref="IViewModel" /> to show.</param>
        /// <param name="context">The specified context.</param>
        /// <param name="parentPresenter">The parent presenter, if any.</param>
        bool Restore([NotNull] IViewModel viewModel, [NotNull] IDataContext context,
            [CanBeNull] IViewModelPresenter parentPresenter);
    }
}