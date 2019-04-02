using MugenMvvm.Attributes;

namespace MugenMvvm.Interfaces.Components
{
    public interface IAttachableComponent
    {
    }

    public interface IAttachableComponent<in T> : IAttachableComponent where T : class
    {
        [Preserve(Conditional = true)]
        void OnAttached(T owner);
    }
}