using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Collections.Components
{
    internal sealed class SealedDecoratorGuard : IConditionComponentCollectionComponent
    {
        public static readonly SealedDecoratorGuard Instance = new();

        private SealedDecoratorGuard()
        {
        }

        public bool CanAdd(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata) => Check(component);

        public bool CanRemove(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata) => Check(component);

        private static bool Check(object component)
        {
            if (component is ICollectionDecorator and not IListenerCollectionDecorator)
                ExceptionManager.ThrowSealedDecorators();
            return true;
        }
    }
}