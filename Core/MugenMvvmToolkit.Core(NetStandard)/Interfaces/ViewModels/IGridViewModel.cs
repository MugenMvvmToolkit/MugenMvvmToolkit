#region Copyright

// ****************************************************************************
// <copyright file="IGridViewModel.cs">
// Copyright (c) 2012-2017 Vyacheslav Volkov
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
using MugenMvvmToolkit.Attributes;
using MugenMvvmToolkit.Interfaces.Collections;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;

namespace MugenMvvmToolkit.Interfaces.ViewModels
{
    public interface IGridViewModel : IViewModel, INotifyCollectionChanging
    {
        [NotNull]
        Type ModelType { get; }

        [NotNull]
        IList OriginalItemsSource { get; }

        [NotNull]
        INotifiableCollection ItemsSource { get; }

        int SelectedIndex { get; set; }

        object SelectedItem { get; set; }

        [CanBeNull]
        FilterDelegate<object> Filter { set; }

        void UpdateItemsSource([CanBeNull] IEnumerable value);

        void UpdateFilter();

        [Preserve(Conditional = true)]
        event EventHandler<IGridViewModel, SelectedItemChangedEventArgs> SelectedItemChanged;

        [Preserve(Conditional = true)]
        event EventHandler<IGridViewModel, ItemsSourceChangedEventArgs> ItemsSourceChanged;
    }

    public interface IGridViewModel<T> : IGridViewModel where T : class
    {
        [NotNull]
        new IList<T> OriginalItemsSource { get; }

        [NotNull]
        new INotifiableCollection<T> ItemsSource { get; }

        new T SelectedItem { get; set; }

        [CanBeNull]
        new FilterDelegate<T> Filter { get; set; }

        void UpdateItemsSource([CanBeNull] IEnumerable<T> value);

        void SetOriginalItemsSource<TItemsSource>([NotNull] TItemsSource originalItemsSource)
            where TItemsSource : IList<T>, INotifyCollectionChanged, IList;

        [Preserve(Conditional = true)]
        new event EventHandler<IGridViewModel, SelectedItemChangedEventArgs<T>> SelectedItemChanged;

        [Preserve(Conditional = true)]
        new event EventHandler<IGridViewModel, ItemsSourceChangedEventArgs<T>> ItemsSourceChanged;
    }
}
