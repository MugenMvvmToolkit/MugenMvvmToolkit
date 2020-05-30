using System;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.ViewModels.Components;

namespace MugenMvvm.UnitTest.ViewModels.Internal
{
    public class TestViewModelServiceResolverComponent : IViewModelServiceResolverComponent, IHasPriority
    {
        #region Properties

        public int Priority { get; set; }

        public Func<IViewModelBase, object, Type, IReadOnlyMetadataContext?, object?>? TryGetService { get; set; }

        #endregion

        #region Implementation of interfaces

        object? IViewModelServiceResolverComponent.TryGetService<TRequest>(IViewModelBase viewModel, in TRequest request, IReadOnlyMetadataContext? metadata)
        {
            return TryGetService?.Invoke(viewModel, request!, typeof(TRequest), metadata);
        }

        #endregion
    }
}