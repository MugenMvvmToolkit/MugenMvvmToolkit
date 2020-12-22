using MugenMvvm.Bindings.Observation;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Bindings.Interfaces.Core.Components
{
    public interface ITargetValueSetterComponent : IComponent<IBinding>
    {
        bool TrySetTargetValue(IBinding binding, MemberPathLastMember targetMember, object? value, IReadOnlyMetadataContext metadata);
    }
}