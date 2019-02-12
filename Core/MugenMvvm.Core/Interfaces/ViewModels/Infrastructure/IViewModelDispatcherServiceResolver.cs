using System;
using System.Collections.Generic;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.ViewModels.Infrastructure
{
    public interface IViewModelDispatcherServiceResolver
    {
        IReadOnlyList<Type> Services { get; }

        object Resolve(IViewModelBase viewModel, Type service, IReadOnlyMetadataContext metadata);
    }
}