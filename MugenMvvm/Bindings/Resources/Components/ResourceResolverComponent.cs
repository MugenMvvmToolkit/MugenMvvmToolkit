using System.Collections.Generic;
using MugenMvvm.Attributes;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Interfaces.Resources;
using MugenMvvm.Bindings.Interfaces.Resources.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Bindings.Resources.Components
{
    public sealed class ResourceResolverComponent : IResourceResolverComponent, IHasPriority
    {
        [Preserve(Conditional = true)]
        public ResourceResolverComponent()
        {
            Resources = new Dictionary<string, object?>();
        }

        public IDictionary<string, object?> Resources { get; }

        public int Priority { get; set; } = ResourceComponentPriority.ResourceResolver;

        public void Add(string name, object? resource)
        {
            Should.NotBeNull(name, nameof(name));
            Resources[name] = resource;
        }

        public void Remove(string name)
        {
            Should.NotBeNull(name, nameof(name));
            Resources.Remove(name);
        }

        public ResourceResolverResult TryGetResource(IResourceResolver resourceResolver, string name, object? state, IReadOnlyMetadataContext? metadata) =>
            Resources.TryGetValue(name, out var value) ? new ResourceResolverResult(value) : default;
    }
}