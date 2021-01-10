using System.Collections.Generic;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Bindings.Interfaces.Core.Components
{
    public interface IBindingHolderComponent : IComponent<IBindingManager>
    {
        ItemOrIReadOnlyList<IBinding> TryGetBindings(IBindingManager bindingManager, object target, string? path, IReadOnlyMetadataContext? metadata);

        bool TryRegister(IBindingManager bindingManager, object? target, IBinding binding, IReadOnlyMetadataContext? metadata);

        bool TryUnregister(IBindingManager bindingManager, object? target, IBinding binding, IReadOnlyMetadataContext? metadata);
    }
}