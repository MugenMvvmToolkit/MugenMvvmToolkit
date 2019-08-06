using MugenMvvm.Binding.Observers;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Core.Components
{
    public interface ISourceValueInterceptorDataBindingComponent : IComponent<IDataBinding>
    {
        object? InterceptSourceValue(in BindingPathLastMember sourceMembers, object? value, IReadOnlyMetadataContext metadata);
    }
}