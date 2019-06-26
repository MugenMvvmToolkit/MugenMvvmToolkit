namespace MugenMvvm.Interfaces.Components
{
    public interface IComponentOwner<T> where T : class
    {
        bool HasComponents { get; }//todo check, remove?

        IComponentCollection<IComponent<T>> Components { get; }
    }
}