using System.Collections.Generic;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Interfaces.Core.Components
{
    public interface IBindingHolderComponent : IComponent<IBindingManager>
    {
        ItemOrList<IBinding?, IReadOnlyList<IBinding>> TryGetBindings(object target, string? path, IReadOnlyMetadataContext? metadata);

        bool TryRegister(IBinding binding, object? target, IReadOnlyMetadataContext? metadata);

        bool TryUnregister(IBinding binding, IReadOnlyMetadataContext? metadata);
    }
}