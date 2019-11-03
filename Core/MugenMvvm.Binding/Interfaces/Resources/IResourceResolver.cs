using System;
using MugenMvvm.Binding.Interfaces.Converters;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Resources
{
    public interface IResourceResolver : IComponentOwner<IResourceResolver>, IComponent<IBindingManager>
    {
        IResourceValue? TryGetResourceValue(string name, IReadOnlyMetadataContext? metadata = null);

        IBindingValueConverter? TryGetConverter(string name, IReadOnlyMetadataContext? metadata = null);

        Type? TryGetType(string name, IReadOnlyMetadataContext? metadata = null);
    }
}