using System.Collections.Generic;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Components
{
    public interface IComponentCollectionDecorator : IComponent<IComponentCollection>
    {
    }

    public interface IComponentCollectionDecorator<TComponent> : IComponentCollectionDecorator
        where TComponent : class
    {
        void Decorate(IList<TComponent> components, IReadOnlyMetadataContext? metadata);
    }
}