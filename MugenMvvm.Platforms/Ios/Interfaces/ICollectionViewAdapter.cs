using System;
using Foundation;
using MugenMvvm.Interfaces.Internal;

namespace MugenMvvm.Ios.Interfaces
{
    public interface ICollectionViewAdapter : IWeakItem
    {
        void ReloadData(Action completion);

        void PerformUpdates(Action updates, Action<bool> completion);

        void InsertItems(NSIndexPath[] paths);

        void DeleteItems(NSIndexPath[] paths);

        void ReloadItems(NSIndexPath[] paths);

        void MoveItem(NSIndexPath path, NSIndexPath newPath);
    }
}