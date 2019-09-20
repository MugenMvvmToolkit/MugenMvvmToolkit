namespace MugenMvvm.Interfaces.Components
{
    //marker for components
    public interface IComponent { }

    public interface IComponent<out TContainer> : IComponent where TContainer : class
    {
    }
}