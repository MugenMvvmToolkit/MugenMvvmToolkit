using System;
using MugenMvvm.Binding.Interfaces.Converters;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Resources
{
    public interface IResourceResolver : IComponentOwner<IResourceResolver>, IComponent<IBindingManager>
    {
        IResourceValue? TryGetResourceValue<TRequest>(string name, in TRequest request, IReadOnlyMetadataContext? metadata = null);

        IBindingValueConverter? TryGetConverter<TRequest>(string name, in TRequest request, IReadOnlyMetadataContext? metadata = null);

        Type? TryGetType<TRequest>(string name, in TRequest request, IReadOnlyMetadataContext? metadata = null);
    }
}