using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Threading;
using MugenMvvm.Internal;

namespace MugenMvvm.Collections
{
    public abstract class BindableCollectionAdapterBase<T> : Collection<T>, INotifyCollectionChanged, INotifyPropertyChanged,
        IThreadDispatcherHandler<BindableCollectionAdapterBase<T>.CollectionChangedEvent>, IValueHolder<Delegate>
    {
        #region Fields

        private readonly IThreadDispatcher? _threadDispatcher;
        private int _suspendCount;
        private WeakListener? _weakListener;

        #endregion

        #region Constructors

        protected BindableCollectionAdapterBase(IThreadDispatcher? threadDispatcher = null, IList<T>? sourceCollection = null,
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

        public IEnumerable? Collection { get; private set; }

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

        void IThreadDispatcherHandler<CollectionChangedEvent>.Execute(in CollectionChangedEvent state)
        {
            AddOrRaiseEvent(state);
        }

        #endregion

        #region Methods

        public void Attach(IEnumerable? collection)
        {
            if (ReferenceEquals(collection, Collection))
                return;
            if (Collection != null)
                OnDetach();
            if (collection != null && !ReferenceEquals(collection, Collection))
                OnAttach(collection);
        }

        protected void BeginBatchUpdate()
        {
            ThreadDispatcher.Execute(ExecutionMode, this, o => o.OnBeginBatchUpdateImpl());
        }

        protected void EndBatchUpdate()
        {
            ThreadDispatcher.Execute(ExecutionMode, this, o => o.OnEndBatchUpdateImpl());
        }

        protected void OnItemChanged(T item, int index, object? args)
        {
            if (!IgnoreItemChangedEvent)
                RaiseEvent(new CollectionChangedEvent(CollectionChangedAction.Changed, item, item, index, index, args));
        }

        protected void OnAdded(T item, int index)
        {
            RaiseEvent(new CollectionChangedEvent(CollectionChangedAction.Add, item, item, index, index, null));
        }

        protected void OnReplaced(T oldItem, T newItem, int index)
        {
            RaiseEvent(new CollectionChangedEvent(CollectionChangedAction.Replace, oldItem, newItem, index, index, null));
        }

        protected void OnMoved(T item, int oldIndex, int newIndex)
        {
            RaiseEvent(new CollectionChangedEvent(CollectionChangedAction.Move, item, item, oldIndex, newIndex, null));
        }

        protected void OnRemoved(T item, int index)
        {
            RaiseEvent(new CollectionChangedEvent(CollectionChangedAction.Remove, item, item, index, index, null));
        }

        protected void OnReset(IEnumerable<T> items)
        {
            RaiseEvent(new CollectionChangedEvent(CollectionChangedAction.Reset, default!, default!, -1, -1,
                ThreadDispatcher.CanExecuteInline(ExecutionMode) && !IsSuspended ? items : items.ToArray()));
        }

        protected void OnCleared()
        {
            RaiseEvent(new CollectionChangedEvent(CollectionChangedAction.Clear, default!, default!, -1, -1, null));
        }

        protected virtual void AddCollectionListener(IEnumerable collection)
        {
            if (collection is IObservableCollection components)
            {
                _weakListener ??= new WeakListener(this);
                components.Components.Add(_weakListener);
            }
            else if (collection is INotifyCollectionChanged notifyCollectionChanged)
            {
                _weakListener ??= new WeakListener(this);
                notifyCollectionChanged.CollectionChanged += _weakListener.OnCollectionChanged;
            }
        }

        protected virtual void RemoveCollectionListener(IEnumerable collection)
        {
            if (_weakListener == null)
                return;
            if (collection is IObservableCollection hasComponents)
                hasComponents.Components.Remove(_weakListener);
            else if (collection is INotifyCollectionChanged notifyCollectionChanged)
                notifyCollectionChanged.CollectionChanged -= _weakListener.OnCollectionChanged;
        }

        protected virtual void OnAttach(IEnumerable collection)
        {
            BeginBatchUpdate();
            try
            {
                Collection = collection;
                AddCollectionListener(collection);
                OnReset(GetCollectionItems(collection));
            }
            finally
            {
                EndBatchUpdate();
            }
        }

        protected virtual void OnDetach()
        {
            if (Collection == null)
                return;

            BeginBatchUpdate();
            try
            {
                RemoveCollectionListener(Collection);
                Collection = null;
                OnCleared();
            }
            finally
            {
                EndBatchUpdate();
            }
        }

        protected virtual void OnBeginBatchUpdate()
        {
        }

        protected virtual void OnEndBatchUpdateInternal()
        {
            for (var index = 0; index < Events.Count; index++)
                Events[index].Raise(this, true);
            Events.Clear();
        }

        protected virtual void OnItemChanged(T item, int index, object? args, bool batch)
        {
        }

        protected virtual void OnAdded(T item, int index, bool batch)
        {
            Insert(index, item);
            RaisePropertyChanged(Default.CountPropertyChangedArgs);
            RaisePropertyChanged(Default.IndexerPropertyChangedArgs);
            if (HasCollectionChangedListeners)
                RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, BoxingExtensions.Box(item), index));
        }

        protected virtual void OnReplaced(T oldItem, T newItem, int index, bool batch)
        {
            this[index] = newItem;
            RaisePropertyChanged(Default.IndexerPropertyChangedArgs);
            if (HasCollectionChangedListeners)
                RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, BoxingExtensions.Box(newItem), BoxingExtensions.Box(oldItem), index));
        }

        protected virtual void OnMoved(T item, int oldIndex, int newIndex, bool batch)
        {
            RemoveAt(oldIndex);
            Insert(newIndex, item);
            RaisePropertyChanged(Default.IndexerPropertyChangedArgs);
            if (HasCollectionChangedListeners)
                RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, BoxingExtensions.Box(item), newIndex, oldIndex));
        }

        protected virtual void OnRemoved(T item, int index, bool batch)
        {
            RemoveAt(index);
            RaisePropertyChanged(Default.CountPropertyChangedArgs);
            RaisePropertyChanged(Default.IndexerPropertyChangedArgs);
            if (HasCollectionChangedListeners)
                RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, BoxingExtensions.Box(item), index));
        }

        protected virtual void OnReset(IEnumerable<T> items, bool batch)
        {
            Clear();
            foreach (var item in items)
                Add(item);
            RaisePropertyChanged(Default.CountPropertyChangedArgs);
            RaisePropertyChanged(Default.IndexerPropertyChangedArgs);
            RaiseCollectionChanged(Default.ResetCollectionEventArgs);
        }

        protected virtual void OnCleared(bool batch)
        {
            Clear();
            RaisePropertyChanged(Default.CountPropertyChangedArgs);
            RaisePropertyChanged(Default.IndexerPropertyChangedArgs);
            RaiseCollectionChanged(Default.ResetCollectionEventArgs);
        }

        protected void RaiseCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            CollectionChanged?.Invoke(this, args);
        }

        protected void RaisePropertyChanged(PropertyChangedEventArgs args)
        {
            PropertyChanged?.Invoke(this, args);
        }

        protected IEnumerable<T> GetCollectionItems(IEnumerable collection)
        {
            if (collection is IObservableCollection observableCollection)
            {
                var result = observableCollection.DecorateItems();
                return result as IEnumerable<T> ?? result.OfType<T>();
            }

            return collection as IEnumerable<T> ?? collection.Cast<T>();
        }

        private void RaiseEvent(in CollectionChangedEvent collectionChangedEvent)
        {
            if (ThreadDispatcher.CanExecuteInline(ExecutionMode))
                AddOrRaiseEvent(collectionChangedEvent);
            else
                ThreadDispatcher.Execute(ExecutionMode, this, collectionChangedEvent);
        }

        private void AddOrRaiseEvent(in CollectionChangedEvent collectionChangedEvent)
        {
            if (IsSuspended)
            {
                if (IsLockRequired)
                    AddEventWithLock(collectionChangedEvent);
                else
                    AddEvent(collectionChangedEvent);
            }
            else
                collectionChangedEvent.Raise(this, false);
        }

        private void OnBeginBatchUpdateImpl()
        {
            if (Interlocked.Increment(ref _suspendCount) == 1)
                OnBeginBatchUpdate();
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
                        OnAdded((T)e.NewItems[0]!, e.NewStartingIndex);
                    else
                    {
                        for (var i = 0; i < e.NewItems.Count; i++)
                            OnAdded((T)e.NewItems[i]!, e.NewStartingIndex + i);
                    }

                    break;
                case NotifyCollectionChangedAction.Move:
                    OnMoved((T)e.OldItems[0]!, e.OldStartingIndex, e.NewStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    OnRemoved((T)e.OldItems[0]!, e.OldStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    OnReplaced((T)e.OldItems[0]!, (T)e.NewItems[0]!, e.NewStartingIndex);
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
                    return GetCollectionCount() != 0;
                default:
                    ExceptionManager.ThrowEnumOutOfRange(nameof(e.Action), e.Action);
                    return false;
            }
        }

        private int GetCollectionCount()
        {
            if (Collection == null)
                return 0;
            if (Collection is ICollection c)
                return c.Count;
            return Collection.Cast<T>().Count();
        }

        private void AddEvent(in CollectionChangedEvent collectionChangedEvent)
        {
            if (collectionChangedEvent.Action == CollectionChangedAction.Reset || collectionChangedEvent.Action == CollectionChangedAction.Clear)
                Events.Clear();
            Events.Add(collectionChangedEvent);
        }

        private void AddEventWithLock(in CollectionChangedEvent collectionChangedEvent)
        {
            lock (Events)
            {
                AddEvent(collectionChangedEvent);
            }
        }

        #endregion

        #region Nested types

        protected class WeakListener : AttachableComponentBase<IObservableCollection>, ICollectionBatchUpdateListener, ICollectionDecoratorListener, IHasPriority
        {
            #region Fields

            private readonly IWeakReference _reference;

            #endregion

            #region Constructors

            public WeakListener(BindableCollectionAdapterBase<T> adapter)
            {
                _reference = adapter.ToWeakReference();
            }

            #endregion

            #region Properties

            public int Priority { get; set; } = ComponentPriority.PostInitializer;

            #endregion

            #region Implementation of interfaces

            public void OnBeginBatchUpdate(IObservableCollection collection)
            {
                GetAdapter()?.BeginBatchUpdate();
            }

            public void OnEndBatchUpdate(IObservableCollection collection)
            {
                GetAdapter()?.EndBatchUpdate();
            }

            public void OnItemChanged(IObservableCollection collection, object? item, int index, object? args)
            {
                GetAdapter()?.OnItemChanged((T)item!, index, args);
            }

            public void OnAdded(IObservableCollection collection, object? item, int index)
            {
                GetAdapter()?.OnAdded((T)item!, index);
            }

            public void OnReplaced(IObservableCollection collection, object? oldItem, object? newItem, int index)
            {
                GetAdapter()?.OnReplaced((T)oldItem!, (T)newItem!, index);
            }

            public void OnMoved(IObservableCollection collection, object? item, int oldIndex, int newIndex)
            {
                GetAdapter()?.OnMoved((T)item!, oldIndex, newIndex);
            }

            public void OnRemoved(IObservableCollection collection, object? item, int index)
            {
                GetAdapter()?.OnRemoved((T)item!, index);
            }

            public void OnReset(IObservableCollection collection, IEnumerable<object?> items)
            {
                GetAdapter()?.OnReset(items as IEnumerable<T> ?? items.OfType<T>());
            }

            public void OnCleared(IObservableCollection collection)
            {
                GetAdapter()?.OnCleared();
            }

            #endregion

            #region Methods

            protected override void OnAttached(IObservableCollection owner, IReadOnlyMetadataContext? metadata)
            {
                owner.GetOrAddCollectionDecoratorManager();
            }

            public void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
            {
                var adapter = GetAdapter();
                if (adapter == null)
                    ((INotifyCollectionChanged)sender).CollectionChanged -= OnCollectionChanged;
                else
                    adapter.OnCollectionChanged(sender, args);
            }

            private BindableCollectionAdapterBase<T>? GetAdapter()
            {
                var referenceTarget = _reference.Target;
                if (referenceTarget == null)
                    OwnerOptional?.Components.Remove(this);
                return (BindableCollectionAdapterBase<T>?)referenceTarget;
            }

            #endregion
        }

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

            public readonly void Raise(BindableCollectionAdapterBase<T> listener, bool batch)
            {
                switch (Action)
                {
                    case CollectionChangedAction.Add:
                        listener.OnAdded(NewItem, NewIndex, batch);
                        break;
                    case CollectionChangedAction.Move:
                        listener.OnMoved(NewItem, OldIndex, NewIndex, batch);
                        break;
                    case CollectionChangedAction.Remove:
                        listener.OnRemoved(OldItem, OldIndex, batch);
                        break;
                    case CollectionChangedAction.Replace:
                        listener.OnReplaced(OldItem, NewItem, OldIndex, batch);
                        break;
                    case CollectionChangedAction.Clear:
                        listener.OnCleared(batch);
                        break;
                    case CollectionChangedAction.Reset:
                        listener.OnReset((IEnumerable<T>)State!, batch);
                        break;
                    case CollectionChangedAction.Changed:
                        listener.OnItemChanged(OldItem, OldIndex, State, batch);
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