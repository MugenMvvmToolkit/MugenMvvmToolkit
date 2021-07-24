using System;
using System.Collections;
using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Collections
{
    public interface IReadOnlyObservableCollection : IComponentOwner<IReadOnlyObservableCollection>, IEnumerable, IDisposable
    {
        Type ItemType { get; }

        int Count { get; }

        object? this[int index] { get; }
    }
}