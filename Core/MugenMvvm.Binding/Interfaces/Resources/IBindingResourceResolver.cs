using System;
using System.Collections.Generic;
using MugenMvvm.Binding.Core;
using MugenMvvm.Binding.Interfaces.Converters;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Resources
{
    public interface IBindingResourceResolver : IComponentOwner<IBindingResourceResolver>, IComponent<IBindingManager>
    {
        IComponent<IDataBinding>? TryGetComponent(string name, IReadOnlyMetadataContext? metadata = null);

        IBindingResource? TryGetBindingResource(string name, IReadOnlyMetadataContext? metadata);

        IBindingValueConverter? TryGetConverter(string name, IReadOnlyMetadataContext? metadata);

        Type? TryGetType(string name, IReadOnlyMetadataContext? metadata);

        IReadOnlyList<BindingType> GetKnownTypes();
    }
}