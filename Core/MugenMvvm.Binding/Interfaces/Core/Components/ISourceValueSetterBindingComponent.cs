using MugenMvvm.Binding.Observers;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Core.Components
{
    public interface ISourceValueSetterBindingComponent : IComponent<IBinding>
    {
        bool TrySetSourceValue(in BindingPathLastMember sourceMembers, object? newValue, IReadOnlyMetadataContext metadata);
    }
}