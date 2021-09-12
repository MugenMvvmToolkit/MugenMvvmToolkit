using System;
using System.Collections;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Collections
{
    public interface IReadOnlyObservableCollection : IComponentOwner<IReadOnlyObservableCollection>, IEnumerable, ISynchronizable, IHasDisposeState
    {
        Type ItemType { get; }

        int Count { get; }
    }
}