using System;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models.Components;

namespace MugenMvvm.Tests.Internal
{
    public class TestDisposableComponent<T> : IDisposableComponent<T> where T : class
    {
        public Action<T, IReadOnlyMetadataContext?>? Dispose { get; set; }

        void IDisposableComponent<T>.Dispose(T owner, IReadOnlyMetadataContext? metadata) => Dispose?.Invoke(owner, metadata);
    }
}