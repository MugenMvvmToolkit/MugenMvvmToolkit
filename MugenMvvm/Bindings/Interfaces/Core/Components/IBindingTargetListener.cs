﻿using System;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Bindings.Interfaces.Core.Components
{
    public interface IBindingTargetListener : IComponent<IBinding>
    {
        void OnTargetUpdateFailed(IBinding binding, Exception exception, IReadOnlyMetadataContext metadata);

        void OnTargetUpdateCanceled(IBinding binding, IReadOnlyMetadataContext metadata);

        void OnTargetUpdated(IBinding binding, object? newValue, IReadOnlyMetadataContext metadata);
    }
}