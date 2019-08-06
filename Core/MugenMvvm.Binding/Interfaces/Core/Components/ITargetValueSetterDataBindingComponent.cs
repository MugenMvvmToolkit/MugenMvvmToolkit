using MugenMvvm.Binding.Observers;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Core.Components
{
    public interface ITargetValueSetterDataBindingComponent : IComponent<IDataBinding>
    {
        bool TrySetTargetValue(in BindingPathLastMember targetMembers, object? newValue, IReadOnlyMetadataContext metadata);
    }
}