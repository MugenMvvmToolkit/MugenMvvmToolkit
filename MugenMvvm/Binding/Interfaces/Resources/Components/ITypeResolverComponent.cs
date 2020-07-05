using System;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Resources.Components
{
    public interface ITypeResolverComponent : IComponent<IResourceResolver>
    {
        Type? TryGetType<TState>(IResourceResolver resourceResolver, string name, in TState state, IReadOnlyMetadataContext? metadata);
    }
}