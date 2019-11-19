using MugenMvvm.Binding.Observers;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Core.Components
{
    public interface ITargetValueGetterBindingComponent : IComponent<IBinding>
    {
        bool TryGetTargetValue(IBinding binding, MemberPathLastMember sourceMember, IReadOnlyMetadataContext metadata, out object? value);
    }
}