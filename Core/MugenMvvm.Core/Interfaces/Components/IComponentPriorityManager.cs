using MugenMvvm.Interfaces.App;

namespace MugenMvvm.Interfaces.Components
{
    public interface IComponentPriorityManager : IComponent<IMugenApplication>
    {
        int GetPriority(object component, object? owner);
    }
}