using System.Collections.Generic;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Components
{
    public interface IDecoratorComponentCollectionComponent : IComponent<IComponentCollection>
    {
    }

    public interface IDecoratorComponentCollectionComponent<TComponent> : IDecoratorComponentCollectionComponent
        where TComponent : class
    {
        void Decorate(IList<TComponent> components, IReadOnlyMetadataContext? metadata);
    }
}