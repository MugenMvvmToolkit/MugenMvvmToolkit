using System;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Wrapping;
using MugenMvvm.Interfaces.Wrapping.Components;
using Should;

namespace MugenMvvm.UnitTests.Wrapping.Internal
{
    public class TestWrapperManagerListener : IWrapperManagerListener
    {
        private readonly IWrapperManager? _wrapperManager;

        public TestWrapperManagerListener(IWrapperManager? wrapperManager = null)
        {
            _wrapperManager = wrapperManager;
        }

        public Action<object, object, IReadOnlyMetadataContext?>? OnWrapped { get; set; }

        void IWrapperManagerListener.OnWrapped(IWrapperManager wrapperManager, object wrapper, object request, IReadOnlyMetadataContext? metadata)
        {
            _wrapperManager?.ShouldEqual(wrapperManager);
            OnWrapped?.Invoke(wrapper, request, metadata);
        }
    }
}