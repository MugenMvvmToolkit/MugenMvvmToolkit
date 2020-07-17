using System;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Wrapping;
using MugenMvvm.Interfaces.Wrapping.Components;
using Should;

namespace MugenMvvm.UnitTest.Wrapping.Internal
{
    public class TestWrapperManagerListener : IWrapperManagerListener
    {
        #region Fields

        private readonly IWrapperManager? _wrapperManager;

        #endregion

        #region Constructors

        public TestWrapperManagerListener(IWrapperManager? wrapperManager = null)
        {
            _wrapperManager = wrapperManager;
        }

        #endregion

        #region Properties

        public Action<object, object, IReadOnlyMetadataContext?>? OnWrapped { get; set; }

        #endregion

        #region Implementation of interfaces

        void IWrapperManagerListener.OnWrapped(IWrapperManager wrapperManager, object wrapper, object request, IReadOnlyMetadataContext? metadata)
        {
            _wrapperManager?.ShouldEqual(wrapperManager);
            OnWrapped?.Invoke(wrapper, request, metadata);
        }

        #endregion
    }
}