using MugenMvvm.Binding.Observers;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Core.Components
{
    public interface ITargetValueInterceptorDataBindingComponent : IComponent<IDataBinding>
    {
        object? InterceptTargetValue(in BindingPathLastMember targetMembers, object? value, IReadOnlyMetadataContext metadata);
    }
}