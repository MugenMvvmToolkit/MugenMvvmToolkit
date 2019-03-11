namespace MugenMvvm.Interfaces.Components
{
    public interface IAttachableComponent<in TContainer> where TContainer : class
    {
        void OnAttached(TContainer container);
    }
}