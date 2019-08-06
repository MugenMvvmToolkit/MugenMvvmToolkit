using System.Collections.Generic;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Components
{
    public sealed class ComponentTracker<TComponent, TComponentListener> : List<TComponent>, IComponentCollectionChangedListener<TComponentListener>
        where TComponent : class
        where TComponentListener : class
    {
        #region Implementation of interfaces

        int IComponent.GetPriority(object source)
        {
            return 0;
        }

        public void OnAdded(IComponentCollection<TComponentListener> collection, TComponentListener component, IReadOnlyMetadataContext metadata)
        {
            if (component is TComponent c)
                Add(c);
        }

        public void OnRemoved(IComponentCollection<TComponentListener> collection, TComponentListener component, IReadOnlyMetadataContext metadata)
        {
            if (component is TComponent c)
                Remove(c);
        }

        public void OnCleared(IComponentCollection<TComponentListener> collection, TComponentListener[] oldItems, IReadOnlyMetadataContext metadata)
        {
            foreach (var item in oldItems)
            {
                if (item is TComponent c)
                    Remove(c);
            }
        }

        #endregion
    }
}