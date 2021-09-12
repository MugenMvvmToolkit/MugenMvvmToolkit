using System;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models.Components;

namespace MugenMvvm.Tests.Internal
{
    public class TestDisposableComponent<T> : IDisposableComponent<T> where T : class
    {
        public Action<T, IReadOnlyMetadataContext?>? OnDisposing { get; set; }
        
        public Action<T, IReadOnlyMetadataContext?>? OnDisposed { get; set; }

        void IDisposableComponent<T>.OnDisposing(T owner, IReadOnlyMetadataContext? metadata) => OnDisposing?.Invoke(owner, metadata);

        void IDisposableComponent<T>.OnDisposed(T owner, IReadOnlyMetadataContext? metadata) => OnDisposed?.Invoke(owner, metadata);
    }
}