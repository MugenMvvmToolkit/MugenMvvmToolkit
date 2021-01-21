using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Bindings.Interfaces.Observation.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Bindings.Observation.Components
{
    public sealed class MemberPathProvider : IMemberPathProviderComponent, IHasPriority
    {
        public int Priority { get; set; } = ObservationComponentPriority.PathProvider;

        public IMemberPath? TryGetMemberPath(IObservationManager observationManager, object path, IReadOnlyMetadataContext? metadata)
        {
            if (path is string s)
                return MemberPath.Get(s);
            return null;
        }
    }
}