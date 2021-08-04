using System;
using System.Collections;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Internal;

namespace MugenMvvm.Interfaces.Collections
{
    public interface IReadOnlyObservableCollection : IComponentOwner<IReadOnlyObservableCollection>, IEnumerable, ISynchronizable, IDisposable
    {
        Type ItemType { get; }

        int Count { get; }
    }
}