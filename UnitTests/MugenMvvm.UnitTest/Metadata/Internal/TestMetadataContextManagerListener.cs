using System;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Metadata.Components;
using MugenMvvm.Interfaces.Models;
using Should;

namespace MugenMvvm.UnitTest.Metadata.Internal
{
    public class TestMetadataContextManagerListener : IMetadataContextManagerListener, IHasPriority
    {
        #region Fields

        private readonly IMetadataContextManager? _metadataContextManager;

        #endregion

        #region Constructors

        public TestMetadataContextManagerListener(IMetadataContextManager? metadataContextManager)
        {
            _metadataContextManager = metadataContextManager;
        }

        #endregion

        #region Properties

        public Action<IReadOnlyMetadataContext, object?>? OnReadOnlyContextCreated { get; set; }

        public Action<IMetadataContext, object?>? OnContextCreated { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        void IMetadataContextManagerListener.OnReadOnlyContextCreated(IMetadataContextManager metadataContextManager, IReadOnlyMetadataContext metadataContext, object? target)
        {
            _metadataContextManager?.ShouldEqual(metadataContextManager);
            OnReadOnlyContextCreated?.Invoke(metadataContext, target);
        }

        void IMetadataContextManagerListener.OnContextCreated(IMetadataContextManager metadataContextManager, IMetadataContext metadataContext, object? target)
        {
            _metadataContextManager?.ShouldEqual(metadataContextManager);
            OnContextCreated?.Invoke(metadataContext, target);
        }

        #endregion
    }
}