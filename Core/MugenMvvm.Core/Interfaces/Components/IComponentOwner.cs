namespace MugenMvvm.Interfaces.Components
{
    public interface IComponentOwner<T> where T : class
    {
        bool HasComponents { get; }

        IComponentCollection<IComponent<T>> Components { get; }
    }
}