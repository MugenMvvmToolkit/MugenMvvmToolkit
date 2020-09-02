using Foundation;

namespace MugenMvvm.Ios.Interfaces
{
    public interface ICellTemplateSelector
    {
        void Initialize(object collectionView);

        NSString GetIdentifier(object collectionView, object? item);

        void OnCellCreated(object collectionView, object cell);
    }
}