using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Components
{
    public interface IComponentCollectionDecorator : IComponentCollectionDecoratorBase
    {
        bool CanDecorate<T>(IReadOnlyMetadataContext? metadata) where T : class;

        void Decorate<T>(IComponentCollection collection, ref ItemOrListEditor<T> components, IReadOnlyMetadataContext? metadata) where T : class;
    }
}