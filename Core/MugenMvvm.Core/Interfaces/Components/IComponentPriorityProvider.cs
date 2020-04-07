using MugenMvvm.Interfaces.App;

namespace MugenMvvm.Interfaces.Components
{
    public interface IComponentPriorityProvider : IComponent<IMugenApplication>
    {
        int GetPriority(object component, object? owner);
    }
}