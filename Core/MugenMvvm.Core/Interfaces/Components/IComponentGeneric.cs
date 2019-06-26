namespace MugenMvvm.Interfaces.Components
{
    public interface IComponent<out TContainer> : IComponent where TContainer : class
    {
    }
}