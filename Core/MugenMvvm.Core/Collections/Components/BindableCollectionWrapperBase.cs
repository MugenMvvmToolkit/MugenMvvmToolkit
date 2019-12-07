using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Threading;

namespace MugenMvvm.Collections.Components
{
    public abstract class BindableCollectionWrapperBase<T> : Collection<T>, INotifyCollectionChanged, INotifyPropertyChanged,
        IObservableCollectionBatchUpdateListener<T>, IThreadDispatcherHandler<BindableCollectionWrapperBase<T>.CollectionChangedEvent>, IValueHolder<Delegate>
    {
        #region Fields

        private readonly IThreadDispatcher? _threadDispatcher;
        private int _suspendCount;

        #endregion

        #region Constructors

        protected BindableCollectionWrapperBase(IThreadDispatcher? threadDispatcher = null, IList<T>? sourceCollection = null,
            ThreadExecutionMode? executionMode = null, bool ignoreItemChangedEvent = true)
            : base(sourceCollection ?? new List<T>())
        {
            _threadDispatcher = threadDispatcher;
            Events = new List<CollectionChangedEvent>();
            ExecutionMode = executionMode ?? ThreadExecutionMode.Main;
            IgnoreItemChangedEvent = ignoreItemChangedEvent;
        }

        #endregion

        #region Properties

        public ICollection<T>? WrappedCollection { get; private set; }

        protected List<CollectionChangedEvent> Events { get; }

        protected IThreadDispatcher ThreadDispatcher => _threadDispatcher.DefaultIfNull();

        protected bool IgnoreItemChangedEvent { get; set; }

        protected bool IsSuspended => _suspendCount != 0;

        protected bool HasCollectionChangedListeners => CollectionChanged != null;

        protected bool HasPropertyChangedListeners => PropertyChanged != null;

        protected ThreadExecutionMode ExecutionMode { get; }

        protected virtual bool IsLockRequired => ExecutionMode != ThreadExecutionMode.Main && ExecutionMode != ThreadExecutionMode.MainAsync;

        Delegate? IValueHolder<Delegate>.Value { get; set; }

        #endregion

        #region Events

        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        public event PropertyChangedEventHandler? PropertyChanged;

        #endregion

        #region Implementation of interfaces

        void IObservableCollectionBatchUpdateListener<T>.OnBeginBatchUpdate(IObservableCollection<T> collection)
        {
            OnBeginBatchUpdate();
        }

        void IObservableCollectionBatchUpdateListener<T>.OnEndBatchUpdate(IObservableCollection<T> collection)
        {
            OnEndBatchUpdate();
        }

        void IThreadDispatcherHandler<CollectionChangedEvent>.Execute(CollectionChangedEvent state)
        {
            AddEventInternal(ref state);
        }

        #endregion

        #region Methods

        public void OnItemChanged(IObservableCollection<T> collection, T item, int index, object? args)
        {
            if (!IgnoreItemChangedEvent)
                OnItemChanged(item, index, args);
        }

        public void OnAdded(IObservableCollection<T> collection, T item, int index)
        {
            OnAdded(item, index);
        }

        public void OnReplaced(IObservableCollection<T> collection, T oldItem, T newItem, int index)
        {
            OnReplaced(oldItem, newItem, index);
        }

        public void OnMoved(IObservableCollection<T> collection, T item, int oldIndex, int newIndex)
        {
            OnMoved(item, oldIndex, newIndex);
        }

        public void OnRemoved(IObservableCollection<T> collection, T item, int index)
        {
            OnRemoved(item, index);
        }

        public void OnReset(IObservableCollection<T> collection, IEnumerable<T> items)
        {
            OnReset(items);
        }

        public void OnCleared(IObservableCollection<T> collection)
        {
            OnCleared();
        }

        public void Attach(ICollection<T> wrappedCollection)
        {
            Should.NotBeNull(wrappedCollection, nameof(wrappedCollection));
            OnAttach(wrappedCollection);
        }

        public void Detach()
        {
            OnDetach();
        }

        protected void OnBeginBatchUpdate()
        {
            ThreadDispatcher.Execute(ExecutionMode, o => o.OnBeginBatchUpdateImpl(), this);
        }

        protected void OnEndBatchUpdate()
        {
            ThreadDispatcher.Execute(ExecutionMode, o => o.OnEndBatchUpdateImpl(), this);
        }

        protected void OnItemChanged(T item, int index, object? args)
        {
            var e = new CollectionChangedEvent(CollectionChangedAction.Changed, item, item, index, index, args);
            AddEvent(ref e);
        }

        protected void OnAdded(T item, int index)
        {
            var e = new CollectionChangedEvent(CollectionChangedAction.Add, item, item, index, index, null);
            AddEvent(ref e);
        }

        protected void OnReplaced(T oldItem, T newItem, int index)
        {
            var e = new CollectionChangedEvent(CollectionChangedAction.Replace, oldItem, newItem, index, index, null);
            AddEvent(ref e);
        }

        protected void OnMoved(T item, int oldIndex, int newIndex)
        {
            var e = new CollectionChangedEvent(CollectionChangedAction.Move, item, item, oldIndex, newIndex, null);
            AddEvent(ref e);
        }

        protected void OnRemoved(T item, int index)
        {
            var e = new CollectionChangedEvent(CollectionChangedAction.Remove, item, item, index, index, null);
            AddEvent(ref e);
        }

        protected void OnReset(IEnumerable<T> items)
        {
            var e = new CollectionChangedEvent(CollectionChangedAction.Reset, default!, default!, -1, -1,
                ThreadDispatcher.CanExecuteInline(ExecutionMode) && !IsSuspended ? items : items.ToArray());
            AddEvent(ref e);
        }

        protected void OnCleared()
        {
            var e = new CollectionChangedEvent(CollectionChangedAction.Clear, default!, default!, -1, -1, null);
            AddEvent(ref e);
        }

        protected virtual void OnAttach(ICollection<T> wrappedCollection)
        {
            OnBeginBatchUpdate();
            try
            {
                Detach();
                WrappedCollection = wrappedCollection;
                if (wrappedCollection is IComponentOwner<IObservableCollection<T>> components)
                    components.AddComponent(this);
                else if (wrappedCollection is INotifyCollectionChanged notifyCollectionChanged)
                    notifyCollectionChanged.CollectionChanged += OnCollectionChanged;
                OnReset(GetCollectionItems());
            }
            finally
            {
                OnEndBatchUpdate();
            }
        }

        protected virtual void OnDetach()
        {
            if (WrappedCollection == null)
                return;

            OnBeginBatchUpdate();
            try
            {
                if (WrappedCollection is IComponentOwner<IObservableCollection<T>> hasComponents)
                    hasComponents.RemoveComponent(this);
                else if (WrappedCollection is INotifyCollectionChanged notifyCollectionChanged)
                    notifyCollectionChanged.CollectionChanged -= OnCollectionChanged;
                WrappedCollection = null;
                OnCleared();
            }
            finally
            {
                OnEndBatchUpdate();
            }
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
                if (IsLockRequired)
                    AddEventWithLock(ref collectionChangedEvent);
                else
                    AddEventRaw(ref collectionChangedEvent);
            }
            else
                collectionChangedEvent.Raise(this, false);
        }

        protected virtual void OnItemChangedInternal(T item, int index, object? args, bool batch)
        {
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
                RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newItem, oldItem, index));
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
            return (WrappedCollection as IObservableCollection<T>)?.DecorateItems() ?? WrappedCollection ?? Default.EmptyArray<T>();
        }

        private void AddEventRaw(ref CollectionChangedEvent collectionChangedEvent)
        {
            if (collectionChangedEvent.Action == CollectionChangedAction.Reset || collectionChangedEvent.Action == CollectionChangedAction.Clear)
                Events.Clear();
            Events.Add(collectionChangedEvent);
        }

        private void AddEvent(ref CollectionChangedEvent collectionChangedEvent)
        {
            if (ThreadDispatcher.CanExecuteInline(ExecutionMode))
                AddEventInternal(ref collectionChangedEvent);
            else
                ThreadDispatcher.Execute(ExecutionMode, this, collectionChangedEvent);
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
                OnReset((IEnumerable<T>) sender);
                return;
            }

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewItems.Count == 1)
                        OnAdded((T) e.NewItems[0], e.NewStartingIndex);
                    else
                    {
                        for (var i = 0; i < e.NewItems.Count; i++)
                            OnAdded((T) e.NewItems[i], e.NewStartingIndex + i);
                    }

                    break;
                case NotifyCollectionChangedAction.Move:
                    OnMoved((T) e.OldItems[0], e.OldStartingIndex, e.NewStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    OnRemoved((T) e.OldItems[0], e.OldStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    OnReplaced((T) e.OldItems[0], (T) e.NewItems[0], e.NewStartingIndex);
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
                    return WrappedCollection != null && WrappedCollection.Count != 0;
                default:
                    ExceptionManager.ThrowEnumOutOfRange(nameof(e.Action), e.Action);
                    return false;
            }
        }

        private void AddEventWithLock(ref CollectionChangedEvent collectionChangedEvent)
        {
            lock (Events)
            {
                AddEventRaw(ref collectionChangedEvent);
            }
        }

        #endregion

        #region Nested types

        [StructLayout(LayoutKind.Auto)]
        protected struct CollectionChangedEvent
        {
            #region Fields

            public CollectionChangedAction Action;
            public int NewIndex;
            public T NewItem;
            public int OldIndex;
            public T OldItem;
            public object? State;

            #endregion

            #region Constructors

            public CollectionChangedEvent(CollectionChangedAction action, T oldItem, T newItem, int oldIndex, int newIndex, object? state)
            {
                Action = action;
                OldItem = oldItem;
                NewItem = newItem;
                OldIndex = oldIndex;
                NewIndex = newIndex;
                State = state;
            }

            #endregion

            #region Implementation of interfaces

            public readonly void Raise(BindableCollectionWrapperBase<T> listener, bool batch)
            {
                switch (Action)
                {
                    case CollectionChangedAction.Add:
                        listener.OnAddedInternal(NewItem, NewIndex, batch);
                        break;
                    case CollectionChangedAction.Move:
                        listener.OnMovedInternal(NewItem, OldIndex, NewIndex, batch);
                        break;
                    case CollectionChangedAction.Remove:
                        listener.OnRemovedInternal(OldItem, OldIndex, batch);
                        break;
                    case CollectionChangedAction.Replace:
                        listener.OnReplacedInternal(OldItem, NewItem, OldIndex, batch);
                        break;
                    case CollectionChangedAction.Clear:
                        listener.OnClearedInternal(batch);
                        break;
                    case CollectionChangedAction.Reset:
                        listener.OnResetInternal((IEnumerable<T>) State!, batch);
                        break;
                    case CollectionChangedAction.Changed:
                        listener.OnItemChangedInternal(OldItem, OldIndex, State, batch);
                        break;
                    default:
                        ExceptionManager.ThrowEnumOutOfRange(nameof(Action), Action);
                        break;
                }
            }

            #endregion
        }

        protected enum CollectionChangedAction
        {
            Add = 1,
            Move = 2,
            Remove = 3,
            Replace = 4,
            Clear = 5,
            Reset = 6,
            Changed = 7
        }

        #endregion
    }
}