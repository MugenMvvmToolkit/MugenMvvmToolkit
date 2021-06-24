using System;
using System.Collections;
using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Collections
{
    public interface IReadOnlyObservableCollection : IEnumerable, IComponentOwner<IReadOnlyObservableCollection>
    {
        Type ItemType { get; }

        int Count { get; }

        object? this[int index] { get; }
    }
}