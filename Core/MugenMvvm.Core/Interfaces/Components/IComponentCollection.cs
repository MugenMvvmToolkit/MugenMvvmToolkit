using System.Collections.Generic;

namespace MugenMvvm.Interfaces.Components
{
    public interface IComponentCollection<T> where T : class//todo add/remove event wrapper, target is optional????
    {
        bool HasItems { get; }

        void Add(T item);

        void Remove(T item);

        void Clear();

        IReadOnlyList<T> GetItems();
    }
}