﻿using System;
using System.Collections.Generic;
using MugenMvvm.Binding.Interfaces.Converters;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Resources;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Resources
{
    public interface IResourceResolver : IComponentOwner<IResourceResolver>, IComponent<IBindingManager>
    {
        IComponent<IBinding>? TryGetComponent(string name, IReadOnlyMetadataContext? metadata = null);

        IResourceValue? TryGetResourceValue(string name, IReadOnlyMetadataContext? metadata = null);

        IValueConverter? TryGetConverter(string name, IReadOnlyMetadataContext? metadata = null);

        Type? TryGetType(string name, IReadOnlyMetadataContext? metadata = null);

        IReadOnlyList<KnownType> GetKnownTypes();
    }
}