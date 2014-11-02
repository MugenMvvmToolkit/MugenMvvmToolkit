#region Copyright
// ****************************************************************************
// <copyright file="IGridViewModel.cs">
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
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces.Collections;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;

namespace MugenMvvmToolkit.Interfaces.ViewModels
{
    /// <summary>
    ///     Represents the interface for view models that have a collection of any objects.
    /// </summary>
    public interface IGridViewModel : IViewModel, INotifyCollectionChanging
    {
        /// <summary>
        ///     Gets the type of model.
        /// </summary>
        [NotNull]
        Type ModelType { get; }

        /// <summary>
        ///     Gets the original collection of items source without the filter.
        /// </summary>
        [NotNull]
        IList OriginalItemsSource { get; }

        /// <summary>
        ///     Gets the collection of objects.
        /// </summary>
        [NotNull]
        IEnumerable ItemsSource { get; }

        /// <summary>
        ///     Gets or sets the selected item.
        /// </summary>
        object SelectedItem { get; set; }

        /// <summary>
        ///     Sets the filter.
        /// </summary>
        [CanBeNull]
        FilterDelegate<object> Filter { set; }

        /// <summary>
        ///     Updates the current <see cref="ItemsSource" />.
        /// </summary>
        /// <param name="value">The new item source value.</param>
        void UpdateItemsSource([CanBeNull] IEnumerable value);

        /// <summary>
        ///     Updates the filter state.
        /// </summary>
        void UpdateFilter();

        /// <summary>
        ///     Occurs when the <c>SelectedItem</c> property changed.
        /// </summary>
        event EventHandler<IGridViewModel, SelectedItemChangedEventArgs> SelectedItemChanged;

        /// <summary>
        ///     Occurs when the <c>ItemsSource</c> property changed.
        /// </summary>
        event EventHandler<IGridViewModel, ItemsSourceChangedEventArgs> ItemsSourceChanged;
    }

    /// <summary>
    ///     Represents the interface for linear lists.
    /// </summary>
    public interface IGridViewModel<T> : IGridViewModel where T : class
    {
        /// <summary>
        ///     Gets the original collection of items source without the filter.
        /// </summary>
        [NotNull]
        new IList<T> OriginalItemsSource { get; }

        /// <summary>
        ///     Gets or sets the collection of objects.
        /// </summary>
        [NotNull]
        new IList<T> ItemsSource { get; }

        /// <summary>
        ///     Gets or sets the selected item.
        /// </summary>
        new T SelectedItem { get; set; }

        /// <summary>
        ///     Gets or sets the filter.
        /// </summary>
        [CanBeNull]
        new FilterDelegate<T> Filter { get; set; }

        /// <summary>
        ///     Updates the current <see cref="ItemsSource" />.
        /// </summary>
        /// <param name="value">The new items source value.</param>
        void UpdateItemsSource([CanBeNull] IEnumerable<T> value);

        /// <summary>
        ///     Sets the original collection of items.
        /// </summary>
        /// <param name="originalItemsSource">The source collection.</param>
        void SetOriginalItemsSource<TItemsSource>([NotNull] TItemsSource originalItemsSource)
            where TItemsSource : IList<T>, INotifyCollectionChanged, IList;

        /// <summary>
        ///     Occurs when the <c>SelectedItem</c> property changed.
        /// </summary>
        new event EventHandler<IGridViewModel, SelectedItemChangedEventArgs<T>> SelectedItemChanged;

        /// <summary>
        ///     Occurs when the <c>ItemsSource</c> property changed.
        /// </summary>
        new event EventHandler<IGridViewModel, ItemsSourceChangedEventArgs<T>> ItemsSourceChanged;
    }
}