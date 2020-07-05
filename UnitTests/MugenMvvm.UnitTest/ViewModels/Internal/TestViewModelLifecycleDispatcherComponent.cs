using System;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.ViewModels.Components;

namespace MugenMvvm.UnitTest.ViewModels.Internal
{
    public class TestViewModelLifecycleDispatcherComponent : IViewModelLifecycleDispatcherComponent, IHasPriority
    {
        #region Properties

        public Action<IViewModelManager, IViewModelBase, ViewModelLifecycleState, object?, Type, IReadOnlyMetadataContext?>? OnLifecycleChanged { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        void IViewModelLifecycleDispatcherComponent.OnLifecycleChanged<TState>(IViewModelManager viewModelManager, IViewModelBase viewModel, ViewModelLifecycleState lifecycleState, in TState state, IReadOnlyMetadataContext? metadata)
        {
            OnLifecycleChanged?.Invoke(viewModelManager, viewModel, lifecycleState, state, typeof(TState), metadata);
        }

        #endregion
    }
}