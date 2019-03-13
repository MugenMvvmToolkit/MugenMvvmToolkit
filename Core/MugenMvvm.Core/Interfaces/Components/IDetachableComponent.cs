using MugenMvvm.Attributes;

namespace MugenMvvm.Interfaces.Components
{
    public interface IDetachableComponent<in T> where T : class
    {
        [Preserve(Conditional = true)]
        void OnDetached(T owner);
    }
}