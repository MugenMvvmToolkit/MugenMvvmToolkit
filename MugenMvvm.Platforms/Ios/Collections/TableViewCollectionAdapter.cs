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
    public class TableViewCollectionAdapter : ICollectionViewAdapter, IThreadDispatcherHandler, IValueHolder<Delegate>
    {
        private readonly IWeakReference _targetRef;
        private readonly IThreadDispatcher? _threadDispatcher;

        public TableViewCollectionAdapter(UITableView tableView, IThreadDispatcher? threadDispatcher = null)
        {
            Should.NotBeNull(tableView, nameof(tableView));
            _threadDispatcher = threadDispatcher;
            _targetRef = tableView.ToWeakReference();
        }

        public UITableView? TableView => (UITableView?) _targetRef.Target;

        public UITableViewRowAnimation InsertRowsAnimation { get; set; } = UITableViewRowAnimation.Automatic;

        public UITableViewRowAnimation DeleteRowsAnimation { get; set; } = UITableViewRowAnimation.Automatic;

        public UITableViewRowAnimation ReloadRowsAnimation { get; set; } = UITableViewRowAnimation.Automatic;

        public bool IsAlive => _targetRef.IsAlive;

        protected IThreadDispatcher ThreadDispatcher => _threadDispatcher.DefaultIfNull();

        Delegate? IValueHolder<Delegate>.Value { get; set; }

        public void ReloadData(Action completion)
        {
            var tableView = TableView;
            if (tableView == null)
                completion();
            else
            {
                tableView.ReloadData();
                ThreadDispatcher.Execute(ThreadExecutionMode.MainAsync, this, completion);
            }
        }

        public void PerformUpdates(Action updates, Action<bool> completion)
        {
            var tableView = TableView;
            if (tableView == null)
            {
                updates();
                completion(false);
            }
            else
                tableView.PerformBatchUpdates(updates, completion);
        }

        public void InsertItems(NSIndexPath[] paths) => TableView?.InsertRows(paths, InsertRowsAnimation);

        public void DeleteItems(NSIndexPath[] paths) => TableView?.DeleteRows(paths, DeleteRowsAnimation);

        public void ReloadItems(NSIndexPath[] paths) => TableView?.ReloadRows(paths, ReloadRowsAnimation);

        public void MoveItem(NSIndexPath path, NSIndexPath newPath) => TableView?.MoveRow(path, newPath);

        void IThreadDispatcherHandler.Execute(object? state)
        {
            TableView?.LayoutIfNeeded();
            ((Action) state!).Invoke();
        }
    }
}