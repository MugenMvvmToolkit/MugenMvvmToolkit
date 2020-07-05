using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Observation.Components
{
    public interface IMemberPathProviderComponent : IComponent<IObservationManager>
    {
        IMemberPath? TryGetMemberPath<TPath>(IObservationManager observationManager, [DisallowNull]in TPath path, IReadOnlyMetadataContext? metadata);
    }
}