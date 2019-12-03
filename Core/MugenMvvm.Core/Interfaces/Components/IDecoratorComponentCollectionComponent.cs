using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Components
{
    public interface IDecoratorComponentCollectionComponent : IComponent<IComponentCollection>
    {
    }

    public interface IDecoratorComponentCollectionComponent<TComponent> : IDecoratorComponentCollectionComponent
        where TComponent : class
    {
        bool TryDecorate(ref TComponent[] components, IReadOnlyMetadataContext? metadata);
    }
}