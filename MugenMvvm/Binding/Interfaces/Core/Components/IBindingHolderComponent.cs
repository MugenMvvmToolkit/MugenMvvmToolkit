using System.Collections.Generic;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Interfaces.Core.Components
{
    public interface IBindingHolderComponent : IComponent<IBindingManager>
    {
        ItemOrList<IBinding, IReadOnlyList<IBinding>> TryGetBindings(IBindingManager bindingManager, object target, string? path, IReadOnlyMetadataContext? metadata);

        bool TryRegister(IBindingManager bindingManager, object? target, IBinding binding, IReadOnlyMetadataContext? metadata);

        bool TryUnregister(IBindingManager bindingManager, object? target, IBinding binding, IReadOnlyMetadataContext? metadata);
    }
}