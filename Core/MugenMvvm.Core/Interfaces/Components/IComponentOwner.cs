namespace MugenMvvm.Interfaces.Components
{
    public interface IComponentOwner
    {
        bool HasComponents { get; }

        IComponentCollection Components { get; }
    }

    public interface IComponentOwner<T> : IComponentOwner where T : class
    {
    }
}