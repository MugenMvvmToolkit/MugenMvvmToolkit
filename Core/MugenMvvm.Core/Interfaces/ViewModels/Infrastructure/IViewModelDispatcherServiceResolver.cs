using System;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.ViewModels.Infrastructure
{
    public interface IViewModelDispatcherServiceResolver
    {
        Type Service { get; }

        object Resolve(IViewModelBase viewModel, IReadOnlyMetadataContext metadata);
    }
}