using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Threading;

namespace MugenMvvm.Collections
{
    public abstract class BindableCollectionWrapperBase<T> : Collection<T>, IObservableCollectionChangedListener, INotifyCollectionChanged, INotifyPropertyChanged
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
            if (wrappedCollection is IHasListeners<IObservableCollectionChangedListener> hasListeners)
                hasListeners.AddListener(this);
            else if (wrappedCollection is INotifyCollectionChanged notifyCollectionChanged)
                notifyCollectionChanged.CollectionChanged += OnCollectionChanged;
            foreach (var item in wrappedCollection)
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

        #endregion

        #region Events

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Implementation of interfaces

        public void OnBeginBatchUpdate(IEnumerable collection)
        {
            if (ThreadDispatcher.IsOnMainThread)
                OnBeginBatchUpdateImpl();
            else
                ThreadDispatcher.Execute(o => ((BindableCollectionWrapperBase<T>)o).OnBeginBatchUpdateImpl(), ThreadExecutionMode.Main, this);
        }

        public void OnEndBatchUpdate(IEnumerable collection)
        {
            if (ThreadDispatcher.IsOnMainThread)
                OnEndBatchUpdateImpl();
            else
                ThreadDispatcher.Execute(o => ((BindableCollectionWrapperBase<T>)o).OnEndBatchUpdateImpl(), ThreadExecutionMode.Main, this);
        }

        void IObservableCollectionChangedListener.OnAdded(IEnumerable collection, object item, int index)
        {
            var e = new CollectionChangedEvent(NotifyCollectionChangedAction.Add, item, item, index, index);
            AddEvent(ref e);
        }

        void IObservableCollectionChangedListener.OnReplaced(IEnumerable collection, object oldItem, object newItem, int index)
        {
            var e = new CollectionChangedEvent(NotifyCollectionChangedAction.Replace, oldItem, newItem, index, index);
            AddEvent(ref e);
        }

        void IObservableCollectionChangedListener.OnMoved(IEnumerable collection, object item, int oldIndex, int newIndex)
        {
            var e = new CollectionChangedEvent(NotifyCollectionChangedAction.Move, item, item, oldIndex, newIndex);
            AddEvent(ref e);
        }

        void IObservableCollectionChangedListener.OnRemoved(IEnumerable collection, object item, int index)
        {
            var e = new CollectionChangedEvent(NotifyCollectionChangedAction.Remove, item, item, index, index);
            AddEvent(ref e);
        }

        void IObservableCollectionChangedListener.OnCleared(IEnumerable collection)
        {
            var e = new CollectionChangedEvent(NotifyCollectionChangedAction.Reset, null, null, -1, -1);
            AddEvent(ref e);
        }

        #endregion

        #region Methods

        public void Unbind()
        {
            if (WrappedCollection is IHasListeners<IObservableCollectionChangedListener> hasListeners)
                hasListeners.RemoveListener(this);
            else if (WrappedCollection is INotifyCollectionChanged notifyCollectionChanged)
                notifyCollectionChanged.CollectionChanged -= OnCollectionChanged;
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

        protected virtual void OnAddedInternal(object item, int index, bool batch)
        {
            Insert(index, (T)item);
            RaisePropertyChanged(Default.CountPropertyChangedArgs);
            RaisePropertyChanged(Default.IndexerPropertyChangedArgs);
            if (HasCollectionChangedListeners)
                RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
        }

        protected virtual void OnReplacedInternal(object oldItem, object newItem, int index, bool batch)
        {
            this[index] = (T)newItem;
            RaisePropertyChanged(Default.IndexerPropertyChangedArgs);
            if (HasCollectionChangedListeners)
                RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, oldItem, newItem, index));
        }

        protected virtual void OnMovedInternal(object item, int oldIndex, int newIndex, bool batch)
        {
            RemoveAt(oldIndex);
            Insert(newIndex, (T)item);
            RaisePropertyChanged(Default.IndexerPropertyChangedArgs);
            if (HasCollectionChangedListeners)
                RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, item, newIndex, oldIndex));
        }

        protected virtual void OnRemovedInternal(object item, int index, bool batch)
        {
            RemoveAt(index);
            RaisePropertyChanged(Default.CountPropertyChangedArgs);
            RaisePropertyChanged(Default.IndexerPropertyChangedArgs);
            if (HasCollectionChangedListeners)
                RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
        }

        protected virtual void OnClearedInternal(bool batch)
        {
            Clear();
            RaisePropertyChanged(Default.CountPropertyChangedArgs);
            RaisePropertyChanged(Default.IndexerPropertyChangedArgs);
            RaiseCollectionChanged(Default.ResetCollectionEventArgs);
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

        protected virtual void RaiseCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            CollectionChanged?.Invoke(this, args);
        }

        protected virtual void RaisePropertyChanged(PropertyChangedEventArgs args)
        {
            PropertyChanged?.Invoke(this, args);
        }

        private void AddEvent(ref CollectionChangedEvent collectionChangedEvent)
        {
            if (ThreadDispatcher.IsOnMainThread)
                AddEventInternal(ref collectionChangedEvent);
            else
                ThreadDispatcher.Execute(collectionChangedEvent, ThreadExecutionMode.Main, this);
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
            var collection = (IEnumerable)sender;
            var isResetEvent = IsResetEvent(collection, e);
            if (ThreadDispatcher.IsOnMainThread || !isResetEvent)
                OnCollectionChangedImpl(collection, e, isResetEvent);
            else
                ThreadDispatcher.Execute(GenerateReset, ThreadExecutionMode.Main, collection.OfType<object>().ToArray());
        }

        private void OnCollectionChangedImpl(IEnumerable collection, NotifyCollectionChangedEventArgs e, bool isReset)
        {
            if (isReset)
            {
                GenerateReset(collection);
                return;
            }

            IObservableCollectionChangedListener listener = this;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewItems.Count == 1)
                        listener.OnAdded(collection, e.NewItems[0], e.NewStartingIndex);
                    else
                    {
                        OnBeginBatchUpdate(collection);
                        try
                        {
                            for (var i = 0; i < e.NewItems.Count; i++)
                                listener.OnAdded(collection, e.NewItems[i], e.NewStartingIndex + i);
                        }
                        finally
                        {
                            OnEndBatchUpdate(collection);
                        }
                    }

                    break;
                case NotifyCollectionChangedAction.Move:
                    listener.OnMoved(collection, e.OldItems[0], e.OldStartingIndex, e.NewStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    listener.OnRemoved(collection, e.OldItems[0], e.OldStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    listener.OnReplaced(collection, e.OldItems[0], e.NewItems[0], e.NewStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    listener.OnCleared(collection);
                    break;
                default:
                    throw ExceptionManager.EnumOutOfRange(nameof(e.Action), e.Action);
            }
        }

        private void GenerateReset(object state)
        {
            var items = (IEnumerable)state;
            IObservableCollectionChangedListener listener = this;
            listener.OnBeginBatchUpdate(items);
            try
            {
                listener.OnCleared(items);
                var index = 0;
                foreach (var item in items)
                    listener.OnAdded(items, item, index++);
            }
            finally
            {
                listener.OnEndBatchUpdate(items);
            }
        }

        private static bool IsResetEvent(IEnumerable items, NotifyCollectionChangedEventArgs e)
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
                    return items.OfType<object>().Any();
                default:
                    throw ExceptionManager.EnumOutOfRange(nameof(e.Action), e.Action);
            }
        }

        #endregion

        #region Nested types

        protected struct CollectionChangedEvent : IThreadDispatcherHandler
        {
            #region Fields

            public readonly NotifyCollectionChangedAction Action;

            public readonly int NewIndex;

            public readonly object? NewItem;

            public readonly int OldIndex;

            public readonly object? OldItem;

            #endregion

            #region Constructors

            public CollectionChangedEvent(NotifyCollectionChangedAction action, object? oldItem, object? newItem, int oldIndex, int newIndex)
            {
                Action = action;
                OldItem = oldItem;
                NewItem = newItem;
                OldIndex = oldIndex;
                NewIndex = newIndex;
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
                        listener.OnClearedInternal(batch);
                        break;
                    default:
                        throw ExceptionManager.EnumOutOfRange(nameof(Action), Action);
                }
            }

            #endregion
        }

        #endregion
    }
}