using System;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.ViewModels.Components;
using Should;

namespace MugenMvvm.UnitTests.ViewModels.Internal
{
    public class TestViewModelLifecycleListener : IViewModelLifecycleListener, IHasPriority
    {
        private readonly IViewModelManager? _viewModelManager;

        public TestViewModelLifecycleListener(IViewModelManager? viewModelManager = null)
        {
            _viewModelManager = viewModelManager;
        }

        public Action<IViewModelBase, ViewModelLifecycleState, object?, IReadOnlyMetadataContext?>? OnLifecycleChanged { get; set; }

        public int Priority { get; set; }

        void IViewModelLifecycleListener.OnLifecycleChanged(IViewModelManager viewModelManager, IViewModelBase viewModel, ViewModelLifecycleState lifecycleState, object? state,
            IReadOnlyMetadataContext? metadata)
        {
            _viewModelManager?.ShouldEqual(viewModelManager);
            OnLifecycleChanged?.Invoke(viewModel, lifecycleState, state, metadata);
        }
    }
}