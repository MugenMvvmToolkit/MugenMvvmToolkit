using System;
using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Resources.Components
{
    public interface ITypeResolverComponent : IComponent<IResourceResolver>
    {
        Type? TryGetType<TState>(string name, [AllowNull]in TState state, IReadOnlyMetadataContext? metadata);
    }
}