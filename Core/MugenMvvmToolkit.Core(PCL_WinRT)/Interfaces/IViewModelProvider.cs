#region Copyright

// ****************************************************************************
// <copyright file="IViewModelProvider.cs">
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

using System;
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;

namespace MugenMvvmToolkit.Interfaces
{
    public interface IViewModelProvider
    {
        [Pure, NotNull]
        IViewModel GetViewModel([NotNull] GetViewModelDelegate<IViewModel> getViewModel,
            [NotNull] IDataContext dataContext);

        [Pure, NotNull]
        IViewModel GetViewModel([NotNull] Type viewModelType, [NotNull] IDataContext dataContext);

        void InitializeViewModel([NotNull] IViewModel viewModel, [CanBeNull] IDataContext dataContext);

        [NotNull, Pure]
        IDataContext PreserveViewModel([NotNull] IViewModel viewModel, [CanBeNull] IDataContext dataContext);

        [Pure]
        IViewModel RestoreViewModel([CanBeNull] IDataContext viewModelState, [CanBeNull] IDataContext dataContext, bool throwOnError);

        [Pure, CanBeNull]
        IViewModel TryGetViewModelById(Guid viewModelId);

        event EventHandler<IViewModelProvider, ViewModelInitializationEventArgs> Initializing;

        event EventHandler<IViewModelProvider, ViewModelInitializationEventArgs> Initialized;

        event EventHandler<IViewModelProvider, ViewModelPreservingEventArgs> Preserving;

        event EventHandler<IViewModelProvider, ViewModelPreservedEventArgs> Preserved;

        event EventHandler<IViewModelProvider, ViewModelRestoringEventArgs> Restoring;

        event EventHandler<IViewModelProvider, ViewModelRestoredEventArgs> Restored;
    }
}
