using System;
using System.Collections;
using JetBrains.Annotations;
using MugenMvvm.Internal;

namespace MugenMvvm.Interfaces.Collections
{
    public interface IObservableCollection : IReadOnlyObservableCollection, IList
    {
        new int Count { get; }

        new object? this[int index] { get; set; }

        [MustUseReturnValue]
        ActionToken BatchUpdate();

        void Move(int oldIndex, int newIndex);

        void Reset(IEnumerable? items);
    }
}