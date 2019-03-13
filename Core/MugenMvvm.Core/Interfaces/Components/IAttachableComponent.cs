using MugenMvvm.Attributes;

namespace MugenMvvm.Interfaces.Components
{
    public interface IAttachableComponent<in T> where T : class
    {
        [Preserve(Conditional = true)]
        void OnAttached(T owner);
    }
}