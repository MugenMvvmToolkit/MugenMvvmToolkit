namespace MugenMvvm.Interfaces.Components
{
    public interface IDetachableComponent<in TContainer> where TContainer : class
    {
        void OnDetached(TContainer container);
    }
}