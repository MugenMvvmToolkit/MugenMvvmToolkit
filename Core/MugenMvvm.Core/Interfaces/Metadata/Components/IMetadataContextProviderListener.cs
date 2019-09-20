﻿using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Metadata.Components
{
    public interface IMetadataContextProviderListener : IComponent<IMetadataContextProvider>
    {
        void OnReadOnlyContextCreated(IMetadataContextProvider provider, IReadOnlyMetadataContext metadataContext, object? target);

        void OnContextCreated(IMetadataContextProvider provider, IMetadataContext metadataContext, object? target);
    }
}