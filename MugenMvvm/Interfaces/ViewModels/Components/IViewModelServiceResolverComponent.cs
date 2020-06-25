using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.ViewModels.Components
{
    public interface IViewModelServiceResolverComponent : IComponent<IViewModelManager>
    {
        [Pure]
        object? TryGetService<TRequest>(IViewModelBase viewModel, [DisallowNull] in TRequest request, IReadOnlyMetadataContext? metadata);
    }
}