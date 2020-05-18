using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Observers.Components
{
    public interface IMemberPathObserverProviderComponent : IComponent<IObserverProvider>
    {
        IMemberPathObserver? TryGetMemberPathObserver<TRequest>(object target, [DisallowNull]in TRequest request, IReadOnlyMetadataContext? metadata);
    }
}