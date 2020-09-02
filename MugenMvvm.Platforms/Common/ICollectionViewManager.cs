using System.Collections;

#if XAMARIN_IOS
// ReSharper disable once CheckNamespace
namespace MugenMvvm.Ios.Interfaces
#else
// ReSharper disable once CheckNamespace
namespace MugenMvvm.Android.Interfaces
#endif
{
    public interface ICollectionViewManager
    {
        IEnumerable? GetItemsSource(object collectionView);

        void SetItemsSource(object collectionView, IEnumerable? value);

        object? GetSelectedItem(object collectionView);

        void SetSelectedItem(object collectionView, object? value);

        void ReloadItem(object collectionView, object? item);
    }
}