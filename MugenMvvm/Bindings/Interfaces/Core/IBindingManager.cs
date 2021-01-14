using MugenMvvm.Bindings.Enums;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Bindings.Interfaces.Core
{
    public interface IBindingManager : IComponentOwner<IBindingManager>
    {
        ItemOrIReadOnlyList<IBindingBuilder> TryParseBindingExpression(object expression, IReadOnlyMetadataContext? metadata = null);

        ItemOrIReadOnlyList<IBinding> GetBindings(object target, string? path = null, IReadOnlyMetadataContext? metadata = null);

        void OnLifecycleChanged(IBinding binding, BindingLifecycleState lifecycleState, object? state = null, IReadOnlyMetadataContext? metadata = null);
    }
}