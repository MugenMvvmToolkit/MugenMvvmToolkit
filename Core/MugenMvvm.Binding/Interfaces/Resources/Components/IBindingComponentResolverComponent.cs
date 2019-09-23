﻿using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Resources.Components
{
    public interface IBindingComponentResolverComponent : IComponent<IResourceResolver>
    {
        IComponent<IBinding>? TryGetComponent(string name, IReadOnlyMetadataContext? metadata);
    }
}