using System;
using System.Collections;
using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Collections
{
    public interface IReadOnlyObservableCollection : IEnumerable, IComponentOwner<ICollection>
    {
        Type ItemType { get; }

        int Count { get; }

        object? this[int index] { get; }
    }
}