using System.Collections.Generic;

namespace MugenMvvm.Interfaces.Collections
{
    public interface IComponentCollection<T> where T : class
    {
        bool HasItems { get; }

        void Add(T item);

        void Remove(T item);

        IReadOnlyList<T> GetItems();
    }
}