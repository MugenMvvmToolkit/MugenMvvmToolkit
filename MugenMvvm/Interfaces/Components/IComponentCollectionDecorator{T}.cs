using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Components
{
    public interface IComponentCollectionDecorator<T> : IComponentCollectionDecoratorBase
        where T : class
    {
        void Decorate(IComponentCollection collection, ref ItemOrListEditor<T> components, IReadOnlyMetadataContext? metadata);
    }
}