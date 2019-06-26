using MugenMvvm.Attributes;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Components
{
    public interface IDetachableComponent
    {
    }

    public interface IDetachableComponent<in T> : IDetachableComponent where T : class
    {
        [Preserve(Conditional = true)]
        void OnDetached(T owner, IReadOnlyMetadataContext? metadata);
    }
}