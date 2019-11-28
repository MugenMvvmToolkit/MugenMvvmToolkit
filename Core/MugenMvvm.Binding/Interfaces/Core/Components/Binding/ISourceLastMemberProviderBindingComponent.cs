using MugenMvvm.Binding.Observers;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Core.Components.Binding
{
    public interface ISourceLastMemberProviderBindingComponent : IComponent<IBinding>
    {
        bool TryGetSourceLastMember(IBinding binding, IReadOnlyMetadataContext metadata, out MemberPathLastMember sourceMember);
    }
}