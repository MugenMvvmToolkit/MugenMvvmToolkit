using System;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Wrapping;
using MugenMvvm.Interfaces.Wrapping.Components;

namespace MugenMvvm.Tests.Wrapping
{
    public class TestWrapperManagerListener : IWrapperManagerListener
    {
        public Action<IWrapperManager, object, object, IReadOnlyMetadataContext?>? OnWrapped { get; set; }

        void IWrapperManagerListener.OnWrapped(IWrapperManager wrapperManager, object wrapper, object request, IReadOnlyMetadataContext? metadata) =>
            OnWrapped?.Invoke(wrapperManager, wrapper, request, metadata);
    }
}