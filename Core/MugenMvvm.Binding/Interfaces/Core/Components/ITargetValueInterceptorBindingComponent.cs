using MugenMvvm.Binding.Observers;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Core.Components
{
    public interface ITargetValueInterceptorBindingComponent : IComponent<IBinding>
    {
        object? InterceptTargetValue(in MemberPathLastMember targetMember, object? value, IReadOnlyMetadataContext metadata);
    }
}