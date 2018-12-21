using System.Collections.Generic;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Models;

namespace MugenMvvm.Interfaces
{
    public interface IReadOnlyContext : IReadOnlyCollection<ContextValue>, IHasMemento
    {
        bool TryGet(IContextKey key, out object? value);

        bool TryGet<T>(IContextKey<T> key, out T value);

        bool Contains(IContextKey key);
    }
}