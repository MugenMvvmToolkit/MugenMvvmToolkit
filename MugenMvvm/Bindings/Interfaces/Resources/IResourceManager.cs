﻿using System;
using MugenMvvm.Bindings.Resources;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Bindings.Interfaces.Resources
{
    public interface IResourceManager : IComponentOwner<IResourceManager>
    {
        ResourceResolverResult TryGetResource(string name, object? state = null, IReadOnlyMetadataContext? metadata = null);

        Type? TryGetType(string name, object? state = null, IReadOnlyMetadataContext? metadata = null);
    }
}