using System;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.ViewModels.Components;

namespace MugenMvvm.UnitTest.ViewModels
{
    public class TestViewModelLifecycleDispatcherComponent : IViewModelLifecycleDispatcherComponent, IHasPriority
    {
        #region Properties

        public Func<IViewModelBase, ViewModelLifecycleState, IReadOnlyMetadataContext?, IReadOnlyMetadataContext?>? OnLifecycleChanged { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        IReadOnlyMetadataContext? IViewModelLifecycleDispatcherComponent.OnLifecycleChanged(IViewModelBase viewModel, ViewModelLifecycleState lifecycleState, IReadOnlyMetadataContext? metadata)
        {
            return OnLifecycleChanged?.Invoke(viewModel, lifecycleState, metadata);
        }

        #endregion
    }
}