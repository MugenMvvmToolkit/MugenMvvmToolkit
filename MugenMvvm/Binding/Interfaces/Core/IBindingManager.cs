using System.Collections.Generic;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Bindings.Interfaces.Core
{
    public interface IBindingManager : IComponentOwner<IBindingManager>
    {
        ItemOrList<IBindingBuilder, IReadOnlyList<IBindingBuilder>> TryParseBindingExpression(object expression, IReadOnlyMetadataContext? metadata = null);

        ItemOrList<IBinding, IReadOnlyList<IBinding>> GetBindings(object target, string? path = null, IReadOnlyMetadataContext? metadata = null);

        void OnLifecycleChanged(IBinding binding, BindingLifecycleState lifecycleState, object? state = null, IReadOnlyMetadataContext? metadata = null);
    }
}