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
    public interface IMultiViewModel : IViewModel
    {
        IViewModel SelectedItem { get; set; }

        [NotNull]
        INotifiableCollection<IViewModel> ItemsSource { get; }

        void AddViewModel([NotNull] IViewModel viewModel, bool setSelected = true);

        Task<bool> RemoveViewModelAsync([NotNull] IViewModel viewModel, object parameter = null);

        void Clear();

        event EventHandler<IMultiViewModel, SelectedItemChangedEventArgs<IViewModel>> SelectedItemChanged;

        event EventHandler<IMultiViewModel, ValueEventArgs<IViewModel>> ViewModelAdded;

        event EventHandler<IMultiViewModel, ValueEventArgs<IViewModel>> ViewModelRemoved;
    }
}
