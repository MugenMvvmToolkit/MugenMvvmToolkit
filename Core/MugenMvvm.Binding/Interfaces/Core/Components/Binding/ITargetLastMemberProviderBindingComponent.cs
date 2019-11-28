using MugenMvvm.Binding.Observers;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Core.Components.Binding
{
    public interface ITargetLastMemberProviderBindingComponent : IComponent<IBinding>
    {
        bool TryGetTargetLastMember(IBinding binding, IReadOnlyMetadataContext metadata, out MemberPathLastMember targetMember);
    }
}