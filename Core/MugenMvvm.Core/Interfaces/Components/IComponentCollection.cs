namespace MugenMvvm.Interfaces.Components
{
    public interface IComponentCollection<T> where T : class
    {
        bool HasItems { get; }

        void Add(T item);

        void Remove(T item);

        void Clear();

        T[] GetItems();
    }
}