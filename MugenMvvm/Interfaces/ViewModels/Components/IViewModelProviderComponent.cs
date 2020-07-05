using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.ViewModels.Components
{
    public interface IViewModelProviderComponent : IComponent<IViewModelManager>
    {
        IViewModelBase? TryGetViewModel<TRequest>(IViewModelManager viewModelManager, [DisallowNull]in TRequest request, IReadOnlyMetadataContext? metadata);
    }
}