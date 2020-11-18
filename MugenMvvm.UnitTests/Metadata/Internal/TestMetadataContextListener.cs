using System;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Metadata.Components;
using MugenMvvm.Interfaces.Models;
using Should;

namespace MugenMvvm.UnitTests.Metadata.Internal
{
    public class TestMetadataContextListener : IMetadataContextListener, IHasPriority
    {
        #region Fields

        private readonly IMetadataContext? _metadataContext;

        #endregion

        #region Constructors

        public TestMetadataContextListener(IMetadataContext? metadataContext)
        {
            _metadataContext = metadataContext;
        }

        #endregion

        #region Properties

        public Action<IMetadataContextKey, object?>? OnAdded { get; set; }

        public Action<IMetadataContextKey, object?, object?>? OnChanged { get; set; }

        public Action<IMetadataContextKey, object?>? OnRemoved { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        void IMetadataContextListener.OnAdded(IMetadataContext metadataContext, IMetadataContextKey key, object? newValue)
        {
            _metadataContext?.ShouldEqual(metadataContext);
            OnAdded?.Invoke(key, newValue);
        }

        void IMetadataContextListener.OnChanged(IMetadataContext metadataContext, IMetadataContextKey key, object? oldValue, object? newValue)
        {
            _metadataContext?.ShouldEqual(metadataContext);
            OnChanged?.Invoke(key, oldValue, newValue);
        }

        void IMetadataContextListener.OnRemoved(IMetadataContext metadataContext, IMetadataContextKey key, object? oldValue)
        {
            _metadataContext?.ShouldEqual(metadataContext);
            OnRemoved?.Invoke(key, oldValue);
        }

        #endregion
    }
}