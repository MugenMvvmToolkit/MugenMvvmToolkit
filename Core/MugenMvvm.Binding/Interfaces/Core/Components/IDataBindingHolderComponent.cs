using System.Collections.Generic;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Interfaces.Core.Components
{
    public interface IDataBindingHolderComponent : IComponent<IBindingManager>
    {
        ItemOrList<IDataBinding?, IReadOnlyList<IDataBinding>> TryGetBindings(object target, string? path, IReadOnlyMetadataContext? metadata);

        bool TryRegister(IDataBinding binding, object? target, IReadOnlyMetadataContext? metadata);

        bool TryUnregister(IDataBinding binding, IReadOnlyMetadataContext? metadata);
    }
}