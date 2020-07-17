using System;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.ViewModels.Components;
using Should;

namespace MugenMvvm.UnitTest.ViewModels.Internal
{
    public class TestViewModelLifecycleDispatcherComponent : IViewModelLifecycleDispatcherComponent, IHasPriority
    {
        #region Fields

        private readonly IViewModelManager? _viewModelManager;

        #endregion

        #region Constructors

        public TestViewModelLifecycleDispatcherComponent(IViewModelManager? viewModelManager = null)
        {
            _viewModelManager = viewModelManager;
        }

        #endregion

        #region Properties

        public Action<IViewModelBase, ViewModelLifecycleState, object?, IReadOnlyMetadataContext?>? OnLifecycleChanged { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        void IViewModelLifecycleDispatcherComponent.OnLifecycleChanged(IViewModelManager viewModelManager, IViewModelBase viewModel, ViewModelLifecycleState lifecycleState, object? state,
            IReadOnlyMetadataContext? metadata)
        {
            _viewModelManager?.ShouldEqual(viewModelManager);
            OnLifecycleChanged?.Invoke(viewModel, lifecycleState, state, metadata);
        }

        #endregion
    }
}