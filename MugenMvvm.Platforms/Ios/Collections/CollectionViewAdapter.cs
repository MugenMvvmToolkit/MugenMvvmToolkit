using System;
using Foundation;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Threading;
using MugenMvvm.Ios.Interfaces;
using UIKit;

namespace MugenMvvm.Ios.Collections
{
    public class CollectionViewAdapter : ICollectionViewAdapter, IThreadDispatcherHandler, IValueHolder<Delegate>
    {
        #region Fields

        private readonly IWeakReference _targetRef;
        private readonly IThreadDispatcher? _threadDispatcher;

        #endregion

        #region Constructors

        public CollectionViewAdapter(UICollectionView collectionView, IThreadDispatcher? threadDispatcher = null)
        {
            Should.NotBeNull(collectionView, nameof(collectionView));
            _threadDispatcher = threadDispatcher;
            _targetRef = collectionView.ToWeakReference();
        }

        #endregion

        #region Properties

        public bool IsAlive => _targetRef.IsAlive;

        public UICollectionView? CollectionView => (UICollectionView?) _targetRef.Target;

        protected IThreadDispatcher ThreadDispatcher => _threadDispatcher.DefaultIfNull();

        Delegate? IValueHolder<Delegate>.Value { get; set; }

        #endregion

        #region Implementation of interfaces

        public void ReloadData(Action completion)
        {
            var collectionView = CollectionView;
            if (collectionView == null)
                completion();
            else
            {
                collectionView.ReloadData();
                ThreadDispatcher.Execute(ThreadExecutionMode.MainAsync, this, completion);
            }
        }

        public void PerformUpdates(Action updates, Action<bool> completion)
        {
            var collectionView = CollectionView;
            if (collectionView == null)
            {
                updates();
                completion(false);
            }
            else
                collectionView.PerformBatchUpdates(updates, new UICompletionHandler(completion));
        }

        public void InsertItems(NSIndexPath[] paths) => CollectionView?.InsertItems(paths);

        public void DeleteItems(NSIndexPath[] paths) => CollectionView?.DeleteItems(paths);

        public void ReloadItems(NSIndexPath[] paths) => CollectionView?.ReloadItems(paths);

        public void MoveItem(NSIndexPath path, NSIndexPath newPath) => CollectionView?.MoveItem(path, newPath);

        void IThreadDispatcherHandler.Execute(object? state)
        {
            CollectionView?.LayoutIfNeeded();
            ((Action) state!).Invoke();
        }

        #endregion
    }
}