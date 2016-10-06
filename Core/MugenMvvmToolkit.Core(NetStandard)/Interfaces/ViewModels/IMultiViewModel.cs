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

using System.Threading.Tasks;
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces.Collections;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;

namespace MugenMvvmToolkit.Interfaces.ViewModels
{
    public interface IMultiViewModel<TViewModel> : IViewModel
        where TViewModel : class, IViewModel
    {
        TViewModel SelectedItem { get; set; }

        [NotNull]
        INotifiableCollection<TViewModel> ItemsSource { get; }

        void AddViewModel([NotNull] TViewModel viewModel, bool setSelected = true);

        Task<bool> RemoveViewModelAsync([NotNull] TViewModel viewModel, object parameter = null);

        void Clear();

        event EventHandler<IMultiViewModel<TViewModel>, SelectedItemChangedEventArgs<TViewModel>> SelectedItemChanged;

        event EventHandler<IMultiViewModel<TViewModel>, ValueEventArgs<TViewModel>> ViewModelAdded;

        event EventHandler<IMultiViewModel<TViewModel>, ValueEventArgs<TViewModel>> ViewModelRemoved;
    }
}
