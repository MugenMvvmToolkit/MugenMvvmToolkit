using System;
using MugenMvvm.Bindings.Interfaces.Resources;
using MugenMvvm.Bindings.Interfaces.Resources.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Tests.Bindings.Resources
{
    public class TestTypeResolverComponent : ITypeResolverComponent, IHasPriority
    {
        public Func<IResourceManager, string, object?, IReadOnlyMetadataContext?, Type?>? TryGetType { get; set; }

        public int Priority { get; set; }

        Type? ITypeResolverComponent.TryGetType(IResourceManager resourceManager, string name, object? state, IReadOnlyMetadataContext? metadata) =>
            TryGetType?.Invoke(resourceManager, name, state, metadata);
    }
}