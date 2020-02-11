using System;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.ViewModels.Components;

namespace MugenMvvm.UnitTest.ViewModels
{
    public class TestViewModelServiceResolverComponent : IViewModelServiceResolverComponent, IHasPriority
    {
        #region Properties

        public int Priority { get; set; }

        public Func<IViewModelBase, Type, IReadOnlyMetadataContext?, object?> TryGetService { get; set; }

        #endregion

        #region Implementation of interfaces

        object? IViewModelServiceResolverComponent.TryGetService(IViewModelBase viewModel, Type service, IReadOnlyMetadataContext? metadata)
        {
            return TryGetService?.Invoke(viewModel, service, metadata);
        }

        #endregion
    }
}