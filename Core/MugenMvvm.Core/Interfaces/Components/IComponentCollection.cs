namespace MugenMvvm.Interfaces.Components
{
    public interface IComponentCollection<T> where T : class
    {
        bool HasItems { get; }

        void Add(T component);

        bool Remove(T component);

        void Clear();

        T[] GetItems();
    }
}