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

        public Func<IViewModelBase, ViewModelLifecycleState, object?, Type, IReadOnlyMetadataContext?, IReadOnlyMetadataContext?>? OnLifecycleChanged { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        IReadOnlyMetadataContext? IViewModelLifecycleDispatcherComponent.OnLifecycleChanged<TState>(IViewModelBase viewModel, ViewModelLifecycleState lifecycleState, in TState state, IReadOnlyMetadataContext? metadata)
        {
            return OnLifecycleChanged?.Invoke(viewModel, lifecycleState, state, typeof(TState), metadata);
        }

        #endregion
    }
}