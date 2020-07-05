using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Observation.Components
{
    public interface IMemberPathObserverProviderComponent : IComponent<IObservationManager>
    {
        IMemberPathObserver? TryGetMemberPathObserver<TRequest>(IObservationManager observationManager, object target, [DisallowNull]in TRequest request, IReadOnlyMetadataContext? metadata);
    }
}