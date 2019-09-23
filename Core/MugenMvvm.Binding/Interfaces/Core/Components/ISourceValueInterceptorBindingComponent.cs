using MugenMvvm.Binding.Observers;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Core.Components
{
    public interface ISourceValueInterceptorBindingComponent : IComponent<IBinding>
    {
        object? InterceptSourceValue(in MemberPathLastMember sourceMember, object? value, IReadOnlyMetadataContext metadata);
    }
}