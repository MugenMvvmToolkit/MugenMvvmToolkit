using System;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Resources
{
    public interface IResourceResolver : IComponentOwner<IResourceResolver>, IComponent<IBindingManager>
    {
        IResourceValue? TryGetResourceValue<TState>(string name, in TState state, IReadOnlyMetadataContext? metadata = null);

        Type? TryGetType<TState>(string name, in TState state, IReadOnlyMetadataContext? metadata = null);
    }
}