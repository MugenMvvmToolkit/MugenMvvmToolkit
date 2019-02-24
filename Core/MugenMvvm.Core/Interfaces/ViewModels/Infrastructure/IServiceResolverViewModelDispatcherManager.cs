using System;
using JetBrains.Annotations;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.ViewModels.Infrastructure
{
    public interface IServiceResolverViewModelDispatcherManager : IViewModelDispatcherManager
    {
        [Pure]
        object? TryGetService(IViewModelDispatcher viewModelDispatcher, IViewModelBase viewModel, Type service, IReadOnlyMetadataContext metadata);
    }
}