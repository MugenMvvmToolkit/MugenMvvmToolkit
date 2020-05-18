using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Resources.Components
{
    public interface IResourceResolverComponent : IComponent<IResourceResolver>
    {
        IResourceValue? TryGetResourceValue<TState>(string name, [AllowNull]in TState state, IReadOnlyMetadataContext? metadata);
    }
}