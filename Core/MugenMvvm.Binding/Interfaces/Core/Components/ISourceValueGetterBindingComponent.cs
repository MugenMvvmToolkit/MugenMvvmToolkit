using MugenMvvm.Binding.Observers;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Core.Components
{
    public interface ISourceValueGetterBindingComponent : IComponent<IBinding>
    {
        bool TryGetSourceValue(IBinding binding, MemberPathLastMember targetMember, IReadOnlyMetadataContext metadata, out object? value);
    }
}