using MugenMvvm.Binding.Observation;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Core.Components.Binding
{
    public interface ISourceValueInterceptorBindingComponent : IComponent<IBinding>
    {
        object? InterceptSourceValue(IBinding binding, MemberPathLastMember sourceMember, object? value, IReadOnlyMetadataContext metadata);
    }
}