using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Threading;

namespace MugenMvvm.Collections
{
    public abstract class BindableCollectionWrapperBase<T> : Collection<T>, IObservableCollectionChangedListener<T>, INotifyCollectionChanged, INotifyPropertyChanged, IDisposable
    {
        #region Fields

        private int _suspendCount;

        #endregion

        #region Constructors

        protected BindableCollectionWrapperBase(IThreadDispatcher threadDispatcher, ICollection<T> wrappedCollection, IList<T>? sourceCollection = null)
            : base(sourceCollection ?? new List<T>())
        {
            Should.NotBeNull(threadDispatcher, nameof(threadDispatcher));
            Should.NotBeNull(wrappedCollection, nameof(wrappedCollection));
            WrappedCollection = wrappedCollection;
            ThreadDispatcher = threadDispatcher;
            Events = new List<CollectionChangedEvent>();
            if (wrappedCollection is IHasListeners<IObservableCollectionChangedListener<T>> hasListeners)
                hasListeners.Listeners.Add(this);
            else if (wrappedCollection is INotifyCollectionChanged notifyCollectionChanged)
                notifyCollectionChanged.CollectionChanged += OnCollectionChanged;
            foreach (var item in GetCollectionItems())
                Items.Add(item);
        }

        #endregion

        #region Properties

        public ICollection<T> WrappedCollection { get; }

        protected List<CollectionChangedEvent> Events { get; }

        protected IThreadDispatcher ThreadDispatcher { get; }

        protected bool IsSuspended => _suspendCount != 0;

        protected bool HasCollectionChangedListeners => CollectionChanged != null;

        protected bool HasPropertyChangedListeners => PropertyChanged != null;

        protected virtual ThreadExecutionMode ExecutionMode => ThreadExecutionMode.Main;

        #endregion

        #region Events

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Implementation of interfaces

        public void Dispose()
        {
            if (WrappedCollection is IHasListeners<IObservableCollectionChangedListener<T>> hasListeners)
                hasListeners.Listeners.Remove(this);
            else if (WrappedCollection is INotifyCollectionChanged notifyCollectionChanged)
                notifyCollectionChanged.CollectionChanged -= OnCollectionChanged;
        }

        void IObservableCollectionChangedListener<T>.OnBeginBatchUpdate(IObservableCollection<T> collection)
        {
            OnBeginBatchUpdate();
        }

        void IObservableCollectionChangedListener<T>.OnEndBatchUpdate(IObservableCollection<T> collection)
        {
            OnEndBatchUpdate();
        }

        void IObservableCollectionChangedListener<T>.OnAdded(IObservableCollection<T> collection, T item, int index)
        {
            OnAdded(item, index);
        }

        void IObservableCollectionChangedListener<T>.OnReplaced(IObservableCollection<T> collection, T oldItem, T newItem, int index)
        {
            OnReplaced(oldItem, newItem, index);
        }

        void IObservableCollectionChangedListener<T>.OnMoved(IObservableCollection<T> collection, T item, int oldIndex, int newIndex)
        {
            OnMoved(item, oldIndex, newIndex);
        }

        void IObservableCollectionChangedListener<T>.OnRemoved(IObservableCollection<T> collection, T item, int index)
        {
            OnRemoved(item, index);
        }

        void IObservableCollectionChangedListener<T>.OnReset(IObservableCollection<T> collection, IEnumerable<T> items)
        {
            OnReset(items);
        }

        void IObservableCollectionChangedListener<T>.OnCleared(IObservableCollection<T> collection)
        {
            OnCleared();
        }

        public int GetPriority(object source)
        {
            return GetPriorityInternal(source);
        }

        #endregion

        #region Methods

        protected void OnBeginBatchUpdate()
        {
            if (ThreadDispatcher.IsOnMainThread)
                OnBeginBatchUpdateImpl();
            else
                ThreadDispatcher.Execute(o => ((BindableCollectionWrapperBase<T>)o).OnBeginBatchUpdateImpl(), ExecutionMode, this);
        }

        protected void OnEndBatchUpdate()
        {
            if (ThreadDispatcher.IsOnMainThread)
                OnEndBatchUpdateImpl();
            else
                ThreadDispatcher.Execute(o => ((BindableCollectionWrapperBase<T>)o).OnEndBatchUpdateImpl(), ExecutionMode, this);
        }

        protected void OnAdded(T item, int index)
        {
            var e = new CollectionChangedEvent(NotifyCollectionChangedAction.Add, item, item, index, index, null);
            AddEvent(ref e);
        }

        protected void OnReplaced(T oldItem, T newItem, int index)
        {
            var e = new CollectionChangedEvent(NotifyCollectionChangedAction.Replace, oldItem, newItem, index, index, null);
            AddEvent(ref e);
        }

        protected void OnMoved(T item, int oldIndex, int newIndex)
        {
            var e = new CollectionChangedEvent(NotifyCollectionChangedAction.Move, item, item, oldIndex, newIndex, null);
            AddEvent(ref e);
        }

        protected void OnRemoved(T item, int index)
        {
            var e = new CollectionChangedEvent(NotifyCollectionChangedAction.Remove, item, item, index, index, null);
            AddEvent(ref e);
        }

        protected void OnReset(IEnumerable<T> items)
        {
            var e = new CollectionChangedEvent(NotifyCollectionChangedAction.Reset, default, default, -1, -1, ThreadDispatcher.IsOnMainThread ? items : items.ToArray());
            AddEvent(ref e);
        }

        protected void OnCleared()
        {
            var e = new CollectionChangedEvent(NotifyCollectionChangedAction.Reset, default, default, -1, -1, null);
            AddEvent(ref e);
        }

        protected virtual int GetPriorityInternal(object source)
        {
            return 0;
        }

        protected virtual void OnBeginBatchUpdateInternal()
        {
        }

        protected virtual void OnEndBatchUpdateInternal()
        {
            for (var index = 0; index < Events.Count; index++)
                Events[index].Raise(this, true);
            Events.Clear();
        }

        protected virtual void AddEventInternal(ref CollectionChangedEvent collectionChangedEvent)
        {
            if (IsSuspended)
            {
                if (collectionChangedEvent.Action == NotifyCollectionChangedAction.Reset)
                    Events.Clear();
                Events.Add(collectionChangedEvent);
            }
            else
                collectionChangedEvent.Raise(this, false);
        }

        protected virtual void OnAddedInternal(T item, int index, bool batch)
        {
            Insert(index, item);
            RaisePropertyChanged(Default.CountPropertyChangedArgs);
            RaisePropertyChanged(Default.IndexerPropertyChangedArgs);
            if (HasCollectionChangedListeners)
                RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
        }

        protected virtual void OnReplacedInternal(T oldItem, T newItem, int index, bool batch)
        {
            this[index] = newItem;
            RaisePropertyChanged(Default.IndexerPropertyChangedArgs);
            if (HasCollectionChangedListeners)
                RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, oldItem, newItem, index));
        }

        protected virtual void OnMovedInternal(T item, int oldIndex, int newIndex, bool batch)
        {
            RemoveAt(oldIndex);
            Insert(newIndex, item);
            RaisePropertyChanged(Default.IndexerPropertyChangedArgs);
            if (HasCollectionChangedListeners)
                RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, item, newIndex, oldIndex));
        }

        protected virtual void OnRemovedInternal(T item, int index, bool batch)
        {
            RemoveAt(index);
            RaisePropertyChanged(Default.CountPropertyChangedArgs);
            RaisePropertyChanged(Default.IndexerPropertyChangedArgs);
            if (HasCollectionChangedListeners)
                RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
        }

        protected virtual void OnResetInternal(IEnumerable<T> items, bool batch)
        {
            Clear();
            foreach (var item in items)
                Add(item);
            RaisePropertyChanged(Default.CountPropertyChangedArgs);
            RaisePropertyChanged(Default.IndexerPropertyChangedArgs);
            RaiseCollectionChanged(Default.ResetCollectionEventArgs);
        }

        protected virtual void OnClearedInternal(bool batch)
        {
            Clear();
            RaisePropertyChanged(Default.CountPropertyChangedArgs);
            RaisePropertyChanged(Default.IndexerPropertyChangedArgs);
            RaiseCollectionChanged(Default.ResetCollectionEventArgs);
        }

        protected virtual void RaiseCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            CollectionChanged?.Invoke(this, args);
        }

        protected virtual void RaisePropertyChanged(PropertyChangedEventArgs args)
        {
            PropertyChanged?.Invoke(this, args);
        }

        protected IEnumerable<T> GetCollectionItems()
        {
            return (WrappedCollection as IObservableCollection<T>)?.DecorateItems() ?? WrappedCollection;
        }

        private void AddEvent(ref CollectionChangedEvent collectionChangedEvent)
        {
            if (ThreadDispatcher.IsOnMainThread)
                AddEventInternal(ref collectionChangedEvent);
            else
                ThreadDispatcher.Execute(collectionChangedEvent, ExecutionMode, this);
        }

        private void OnBeginBatchUpdateImpl()
        {
            if (Interlocked.Increment(ref _suspendCount) == 1)
                OnBeginBatchUpdateInternal();
        }

        private void OnEndBatchUpdateImpl()
        {
            if (Interlocked.Decrement(ref _suspendCount) == 0)
                OnEndBatchUpdateInternal();
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (IsResetEvent(e))
            {
                OnReset((IEnumerable<T>)sender);
                return;
            }

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewItems.Count == 1)
                        OnAdded((T)e.NewItems[0], e.NewStartingIndex);
                    else
                    {
                        for (var i = 0; i < e.NewItems.Count; i++)
                            OnAdded((T)e.NewItems[i], e.NewStartingIndex + i);
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    OnMoved((T)e.OldItems[0], e.OldStartingIndex, e.NewStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    OnRemoved((T)e.OldItems[0], e.OldStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    OnReplaced((T)e.OldItems[0], (T)e.NewItems[0], e.NewStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    OnCleared();
                    break;
                default:
                    ExceptionManager.ThrowEnumOutOfRange(nameof(e.Action), e.Action);
                    break;
            }
        }

        private bool IsResetEvent(NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    return false;
                case NotifyCollectionChangedAction.Move:
                case NotifyCollectionChangedAction.Remove:
                case NotifyCollectionChangedAction.Replace:
                    return e.OldItems.Count != 1;
                case NotifyCollectionChangedAction.Reset:
                    return WrappedCollection.Count != 0;
                default:
                    ExceptionManager.ThrowEnumOutOfRange(nameof(e.Action), e.Action);
                    return false;
            }
        }

        #endregion

        #region Nested types

        [StructLayout(LayoutKind.Auto)]
        protected struct CollectionChangedEvent : IThreadDispatcherHandler
        {
            #region Fields

            public NotifyCollectionChangedAction Action;
            public int NewIndex;
            public T NewItem;
            public int OldIndex;
            public T OldItem;
            public IEnumerable<T> ResetItems;

            #endregion

            #region Constructors

            public CollectionChangedEvent(NotifyCollectionChangedAction action, T oldItem, T newItem, int oldIndex, int newIndex, IEnumerable<T> resetItems)
            {
                Action = action;
                OldItem = oldItem;
                NewItem = newItem;
                OldIndex = oldIndex;
                NewIndex = newIndex;
                ResetItems = resetItems;
            }

            #endregion

            #region Implementation of interfaces

            public void Execute(object? state)
            {
                ((BindableCollectionWrapperBase<T>)state).AddEvent(ref this);
            }

            public void Raise(BindableCollectionWrapperBase<T> listener, bool batch)
            {
                switch (Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        listener.OnAddedInternal(NewItem, NewIndex, batch);
                        break;
                    case NotifyCollectionChangedAction.Move:
                        listener.OnMovedInternal(NewItem, OldIndex, NewIndex, batch);
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        listener.OnRemovedInternal(OldItem, OldIndex, batch);
                        break;
                    case NotifyCollectionChangedAction.Replace:
                        listener.OnReplacedInternal(OldItem, NewItem, OldIndex, batch);
                        break;
                    case NotifyCollectionChangedAction.Reset:
                        if (ResetItems == null)
                            listener.OnClearedInternal(batch);
                        else
                            listener.OnResetInternal(ResetItems, batch);
                        break;
                    default:
                        ExceptionManager.ThrowEnumOutOfRange(nameof(Action), Action);
                        break;
                }
            }

            #endregion
        }

        #endregion
    }
}