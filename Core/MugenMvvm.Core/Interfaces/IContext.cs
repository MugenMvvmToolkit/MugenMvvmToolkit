using System.Collections.Generic;
using MugenMvvm.Models;

namespace MugenMvvm.Interfaces
{
    public interface IContext : IReadOnlyContext
    {
        void Set(IContextKey key, object? value);

        void Set<T>(IContextKey<T> key, T value);

        void Merge(IEnumerable<ContextValue> items);

        bool Remove(IContextKey key);

        void Clear();
    }
}