using System;
using JetBrains.Annotations;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.ViewModels.Infrastructure
{
    public interface IServiceResolverViewModelDispatcherComponent : IViewModelDispatcherComponent
    {
        [Pure]
        object? TryGetService(IViewModelDispatcher dispatcher, IViewModelBase viewModel, Type service, IReadOnlyMetadataContext metadata);
    }
}