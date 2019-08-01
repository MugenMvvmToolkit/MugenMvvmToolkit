using MugenMvvm.Attributes;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Components
{
    public interface IAttachableComponent
    {
    }

    public interface IAttachableComponent<in T> : IAttachableComponent where T : class
    {
        [Preserve(Conditional = true)]
        bool OnAttaching(T owner, IReadOnlyMetadataContext? metadata);

        [Preserve(Conditional = true)]
        void OnAttached(T owner, IReadOnlyMetadataContext? metadata);
    }
}