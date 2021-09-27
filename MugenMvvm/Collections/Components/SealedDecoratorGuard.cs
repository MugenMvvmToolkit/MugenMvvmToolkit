using MugenMvvm.Constants;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Models.Components;
using MugenMvvm.Internal;

namespace MugenMvvm.Collections.Components
{
    internal sealed class SealedDecoratorGuard : IConditionComponentCollectionComponent, IDisposableComponent<IReadOnlyObservableCollection>, IHasPriority, ActionToken.IHandler
    {
        public static readonly SealedDecoratorGuard Instance = new();

        private SealedDecoratorGuard()
        {
        }

        public int Priority => ComponentPriority.Max;

        public bool CanAdd(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata) => Check(component);

        public bool CanRemove(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata) => Check(component);

        public void OnDisposing(IReadOnlyObservableCollection owner, IReadOnlyMetadataContext? metadata) => Detach(owner);

        public void OnDisposed(IReadOnlyObservableCollection owner, IReadOnlyMetadataContext? metadata)
        {
        }

        public void Invoke(object? state1, object? state2) => Detach((IReadOnlyObservableCollection) state1!);

        private static bool Check(object component)
        {
            if (component is ICollectionDecorator and not IListenerCollectionDecorator)
                ExceptionManager.ThrowSealedDecorators();
            return true;
        }

        private void Detach(IReadOnlyObservableCollection collection)
        {
            collection.RemoveComponent(Instance);
            collection.Components.RemoveComponent(Instance);
        }
    }
}