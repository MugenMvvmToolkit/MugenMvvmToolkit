using MugenMvvm.Bindings.Observation;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Bindings.Interfaces.Core.Components.Binding
{
    public interface ITargetValueInterceptorBindingComponent : IComponent<IBinding>
    {
        object? InterceptTargetValue(IBinding binding, MemberPathLastMember targetMember, object? value, IReadOnlyMetadataContext metadata);
    }
}