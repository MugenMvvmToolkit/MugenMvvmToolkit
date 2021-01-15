using System;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Bindings.Interfaces.Resources.Components
{
    public interface ITypeResolverComponent : IComponent<IResourceManager>
    {
        Type? TryGetType(IResourceManager resourceManager, string name, object? state, IReadOnlyMetadataContext? metadata);
    }
}