using System;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Resources.Components
{
    public interface ITypeResolverComponent : IComponent<IResourceResolver>
    {
        Type? TryGetType<TRequest>(string name, in TRequest request, IReadOnlyMetadataContext? metadata);
    }
}