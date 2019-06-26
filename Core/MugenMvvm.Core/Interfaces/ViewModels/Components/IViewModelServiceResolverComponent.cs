using System;
using JetBrains.Annotations;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.ViewModels.Infrastructure
{
    public interface IViewModelServiceResolverComponent : IComponent<IViewModelDispatcher>
    {
        [Pure]
        object? TryGetService(IViewModelBase viewModel, Type service, IReadOnlyMetadataContext metadata);
    }
}