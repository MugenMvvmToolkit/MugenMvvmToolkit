using MugenMvvm.Android.Collections;
using MugenMvvm.Android.Native.Interfaces;

namespace MugenMvvm.Android.Interfaces
{
    public interface IItemsSourceProvider : IItemsSourceProviderBase
    {
        ItemsSourceBindableCollectionAdapter CollectionAdapter { get; }

        object? GetItemAt(int position);

        int IndexOf(object? item);
    }
}