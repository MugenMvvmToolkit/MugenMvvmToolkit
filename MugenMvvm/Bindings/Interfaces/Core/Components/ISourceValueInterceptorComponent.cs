using MugenMvvm.Bindings.Observation;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Bindings.Interfaces.Core.Components
{
    public interface ISourceValueInterceptorComponent : IComponent<IBinding>
    {
        object? InterceptSourceValue(IBinding binding, MemberPathLastMember sourceMember, object? value, IReadOnlyMetadataContext metadata);
    }
}