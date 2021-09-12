using MugenMvvm.Interfaces.Internal;

namespace MugenMvvm.Interfaces.Components
{
    public interface IComponentOwner : IMugenService
    {
        bool HasComponents { get; }

        IComponentCollection Components { get; }
    }

    public interface IComponentOwner<out T> : IComponentOwner where T : class
    {
    }
}