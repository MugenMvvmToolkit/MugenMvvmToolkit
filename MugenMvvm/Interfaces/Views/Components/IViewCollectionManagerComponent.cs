using System.Collections;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Views.Components
{
    public interface IViewCollectionManagerComponent : IComponent<IViewManager>
    {
        bool TryGetItemsSource(IViewManager viewManager, object view, IReadOnlyMetadataContext? metadata, out IEnumerable? itemsSource);

        bool TryGetItemsSourceRaw(IViewManager viewManager, object view, IReadOnlyMetadataContext? metadata, out IEnumerable? itemsSource);

        bool TrySetItemsSource(IViewManager viewManager, object view, IEnumerable? value, IReadOnlyMetadataContext? metadata);
    }
}