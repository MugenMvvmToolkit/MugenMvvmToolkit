#region Copyright

// ****************************************************************************
// <copyright file="IMultiViewModel.cs">
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
using System.Threading.Tasks;
using JetBrains.Annotations;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;

namespace MugenMvvmToolkit.Interfaces.ViewModels
{
    /// <summary>
    ///     Represents the view model interface that allows to manage a collection of <see cref="IViewModel" />.
    /// </summary>
    public interface IMultiViewModel : IViewModel
    {
        /// <summary>
        ///     Gets or sets the selected view-model.
        /// </summary>
        IViewModel SelectedItem { get; set; }

        /// <summary>
        ///     Gets the collection of <see cref="IViewModel" />s.
        /// </summary>
        [NotNull]
        IList<IViewModel> ItemsSource { get; }

        /// <summary>
        ///     Adds the specified <see cref="IViewModel" /> to <see cref="ItemsSource" />.
        /// </summary>
        /// <param name="viewModel">
        ///     The specified <see cref="IViewModel" />.
        /// </param>
        /// <param name="setSelected">Sets the specified <see cref="IViewModel"/> as selected view model.</param>
        void AddViewModel([NotNull] IViewModel viewModel, bool setSelected = true);

        /// <summary>
        ///     Removes the specified <see cref="IViewModel" /> from <see cref="ItemsSource" />.
        /// </summary>
        /// <param name="viewModel">
        ///     The specified <see cref="IViewModel" />.
        /// </param>
        /// <param name="parameter">The specified parameter, if any.</param>
        Task<bool> RemoveViewModelAsync([NotNull] IViewModel viewModel, object parameter = null);

        /// <summary>
        ///     Clears all view models from <see cref="ItemsSource" />.
        /// </summary>
        void Clear();

        /// <summary>
        ///     Occurs when the <c>SelectedItem</c> property changed.
        /// </summary>
        event EventHandler<IMultiViewModel, SelectedItemChangedEventArgs<IViewModel>> SelectedItemChanged;

        /// <summary>
        ///     Occurs when a view model is added.
        /// </summary>
        event EventHandler<IMultiViewModel, ValueEventArgs<IViewModel>> ViewModelAdded;

        /// <summary>
        ///     Occurs when a view model is removed.
        /// </summary>
        event EventHandler<IMultiViewModel, ValueEventArgs<IViewModel>> ViewModelRemoved;
    }
}