#region Copyright

// ****************************************************************************
// <copyright file="IMultiViewModel.cs">
// Copyright (c) 2012-2016 Vyacheslav Volkov
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
using System.Threading.Tasks;
using JetBrains.Annotations;
using MugenMvvmToolkit.Attributes;
using MugenMvvmToolkit.Interfaces.Collections;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;

namespace MugenMvvmToolkit.Interfaces.ViewModels
{
    public interface IMultiViewModel : IViewModel
    {
        [NotNull]
        Type ViewModelType { get; }

        IViewModel SelectedItem { get; set; }

        [NotNull]
        IEnumerable<IViewModel> ItemsSource { get; }

        void AddViewModel([NotNull] IViewModel viewModel, bool setSelected = true);

        void InsertViewModel(int index, [NotNull] IViewModel viewModel, bool setSelected = true);

        Task<bool> RemoveViewModelAsync([NotNull] IViewModel viewModel, IDataContext context = null);

        void Clear();

        [Preserve(Conditional = true)]
        event EventHandler<IMultiViewModel, SelectedItemChangedEventArgs<IViewModel>> SelectedItemChanged;

        event EventHandler<IMultiViewModel, ValueEventArgs<IViewModel>> ViewModelAdded;

        event EventHandler<IMultiViewModel, ValueEventArgs<IViewModel>> ViewModelRemoved;
    }

    public interface IMultiViewModel<TViewModel> : IMultiViewModel
        where TViewModel : class, IViewModel
    {
        new TViewModel SelectedItem { get; set; }

        [NotNull]
        new INotifiableCollection<TViewModel> ItemsSource { get; }

        void AddViewModel([NotNull] TViewModel viewModel, bool setSelected = true);

        void InsertViewModel(int index, [NotNull] TViewModel viewModel, bool setSelected = true);

        Task<bool> RemoveViewModelAsync([NotNull] TViewModel viewModel, IDataContext context = null);

        [Preserve(Conditional = true)]
        new event EventHandler<IMultiViewModel<TViewModel>, SelectedItemChangedEventArgs<TViewModel>> SelectedItemChanged;

        new event EventHandler<IMultiViewModel<TViewModel>, ValueEventArgs<TViewModel>> ViewModelAdded;

        new event EventHandler<IMultiViewModel<TViewModel>, ValueEventArgs<TViewModel>> ViewModelRemoved;
    }
}
