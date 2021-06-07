using System;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.ViewModels.Components;

namespace MugenMvvm.Tests.ViewModels
{
    public class TestViewModelLifecycleListener : IViewModelLifecycleListener, IHasPriority
    {
        public Action<IViewModelManager, IViewModelBase, ViewModelLifecycleState, object?, IReadOnlyMetadataContext?>? OnLifecycleChanged { get; set; }

        public int Priority { get; set; }

        void IViewModelLifecycleListener.OnLifecycleChanged(IViewModelManager viewModelManager, IViewModelBase viewModel, ViewModelLifecycleState lifecycleState, object? state,
            IReadOnlyMetadataContext? metadata) =>
            OnLifecycleChanged?.Invoke(viewModelManager, viewModel, lifecycleState, state, metadata);
    }
}