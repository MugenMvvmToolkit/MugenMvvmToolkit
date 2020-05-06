using System;
using System.Collections.Generic;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Views.Components;

namespace MugenMvvm.UnitTest.Views.Internal
{
    public class TestViewModelViewMappingProviderComponent : IViewModelViewMappingProviderComponent, IHasPriority
    {
        #region Properties

        public Func<object, IReadOnlyMetadataContext?, IReadOnlyList<IViewModelViewMapping>?>? TryGetMappingByView { get; set; }

        public Func<IViewModelBase, IReadOnlyMetadataContext?, IReadOnlyList<IViewModelViewMapping>?>? TryGetMappingByViewModel { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        IReadOnlyList<IViewModelViewMapping>? IViewModelViewMappingProviderComponent.TryGetMappingByView(object view, IReadOnlyMetadataContext? metadata)
        {
            return TryGetMappingByView?.Invoke(view, metadata);
        }

        IReadOnlyList<IViewModelViewMapping>? IViewModelViewMappingProviderComponent.TryGetMappingByViewModel(IViewModelBase viewModel, IReadOnlyMetadataContext? metadata)
        {
            return TryGetMappingByViewModel?.Invoke(viewModel, metadata);
        }

        #endregion
    }
}