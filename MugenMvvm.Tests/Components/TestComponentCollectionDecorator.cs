using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Tests.Components
{
    public delegate void DecorateDelegate<T>(IComponentCollection collection, ref ItemOrListEditor<T> components, IReadOnlyMetadataContext? metadata);

    public class TestComponentCollectionDecorator<TComponent> : IComponentCollectionDecorator, IHasPriority
    {
        public DecorateDelegate<TComponent>? DecorateHandler { get; set; }

        public int Priority { get; set; }

        public void Decorate<T>(IComponentCollection collection, ref ItemOrListEditor<T> components, IReadOnlyMetadataContext? metadata) where T : class
        {
            if (DecorateHandler is DecorateDelegate<T> decorate)
                decorate(collection, ref components, metadata);
        }

        bool IComponentCollectionDecorator.CanDecorate<T>(IReadOnlyMetadataContext? metadata) where T : class => typeof(T) == typeof(TComponent);
    }
}