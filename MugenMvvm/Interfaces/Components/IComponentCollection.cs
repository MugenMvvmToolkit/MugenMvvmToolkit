using System;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Components
{
    public interface IComponentCollection : IComponentOwner<IComponentCollection>, IHasCache
    {
        object Owner { get; }

        int Count { get; }

        object? TryAdd<T>(T state, Func<IComponentCollection, T, IReadOnlyMetadataContext?, object?> tryGetComponent, IReadOnlyMetadataContext? metadata = null);

        bool TryAdd(object component, IReadOnlyMetadataContext? metadata = null);

        bool Remove(object component, IReadOnlyMetadataContext? metadata = null);

        void Clear(IReadOnlyMetadataContext? metadata = null);

        ItemOrArray<T> Get<T>(IReadOnlyMetadataContext? metadata = null) where T : class;
    }
}