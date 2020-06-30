using System;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Metadata.Components;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.UnitTest.Metadata.Internal
{
    public class TestMetadataContextManagerListener : IMetadataContextManagerListener, IHasPriority
    {
        #region Properties

        public Action<IMetadataContextManager, IReadOnlyMetadataContext, object?>? OnReadOnlyContextCreated { get; set; }

        public Action<IMetadataContextManager, IMetadataContext, object?>? OnContextCreated { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        void IMetadataContextManagerListener.OnReadOnlyContextCreated(IMetadataContextManager metadataContextManager, IReadOnlyMetadataContext metadataContext, object? target)
        {
            OnReadOnlyContextCreated?.Invoke(metadataContextManager, metadataContext, target);
        }

        void IMetadataContextManagerListener.OnContextCreated(IMetadataContextManager metadataContextManager, IMetadataContext metadataContext, object? target)
        {
            OnContextCreated?.Invoke(metadataContextManager, metadataContext, target);
        }

        #endregion
    }
}