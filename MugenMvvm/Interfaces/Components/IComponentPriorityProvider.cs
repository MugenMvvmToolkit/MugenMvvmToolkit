namespace MugenMvvm.Interfaces.Components
{
    public interface IComponentPriorityProvider
    {
        int GetPriority(object component, object? owner);
    }
}