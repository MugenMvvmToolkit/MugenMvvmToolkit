using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using MugenMvvm.Internal;

namespace MugenMvvm.Interfaces.Collections
{
    public interface IObservableCollection<T> : IReadOnlyObservableCollection<T>, IList<T>
    {
        new int Count { get; }

        new T this[int index] { get; set; }

        [MustUseReturnValue]
        ActionToken BatchUpdate();

        void Move(int oldIndex, int newIndex);

        void Reset(IEnumerable<T>? items);
    }
}