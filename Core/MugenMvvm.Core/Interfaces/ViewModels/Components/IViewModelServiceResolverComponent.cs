using System;
using JetBrains.Annotations;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.ViewModels.Components
{
    public interface IViewModelServiceResolverComponent : IComponent<IViewModelManager>
    {
        [Pure]
        object? TryGetService(IViewModelBase viewModel, Type service, IReadOnlyMetadataContext? metadata);
    }
}