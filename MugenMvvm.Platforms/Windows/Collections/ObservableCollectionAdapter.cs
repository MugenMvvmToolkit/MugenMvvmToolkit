using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Bindings.Members;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;
#if AVALONIA
using Avalonia.Controls;
using MugenMvvm.Avalonia.Bindings;

namespace MugenMvvm.Avalonia.Collections
#else
using System.Windows.Controls;
using MugenMvvm.Windows.Bindings;

namespace MugenMvvm.Windows.Collections
#endif
{
    public sealed class ObservableCollectionAdapter : ObservableCollection<object?>, ISuspendable, IEventListener, BindableCollectionAdapter.IHasItemChangedSupport
    {
        private bool _isNotificationsDirty;
        private int _suspendCount;

        public ObservableCollectionAdapter(ItemsControl control)
        {
            Should.NotBeNull(control, nameof(control));
            Adapter = new DiffableBindableCollectionAdapter(this)
            {
                DiffableComparer = control.BindableMembers().DiffableEqualityComparer()
            };
            BindableMembers.For<ItemsControl>().DiffableEqualityComparer().TryObserve(control, this);
#if AVALONIA
            control.Items = this;
#else
            control.ItemsSource = this;
#endif
        }

        public DiffableBindableCollectionAdapter Adapter { get; }

        public bool IsSuspended => _suspendCount != 0;

        public static IEnumerable? GetItemsSource(IEnumerable? itemSource)
        {
            if (itemSource is ObservableCollectionAdapter c)
                return c.Adapter.Collection;
            return itemSource;
        }

        public ActionToken Suspend(object? state = null, IReadOnlyMetadataContext? metadata = null)
        {
            Interlocked.Increment(ref _suspendCount);
            return ActionToken.FromDelegate((o, _) => ((ObservableCollectionAdapter)o!).EndSuspendNotifications(), this);
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (IsSuspended)
                _isNotificationsDirty = true;
            else
                base.OnCollectionChanged(e);
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (!IsSuspended)
                base.OnPropertyChanged(e);
        }

        private void EndSuspendNotifications()
        {
            if (Interlocked.Decrement(ref _suspendCount) == 0 && _isNotificationsDirty)
            {
                _isNotificationsDirty = false;
                OnPropertyChanged(Default.CountPropertyChangedArgs);
                OnPropertyChanged(Default.IndexerPropertyChangedArgs);
                OnCollectionChanged(Default.ResetCollectionEventArgs);
            }
        }

        bool IEventListener.TryHandle(object? sender, object? message, IReadOnlyMetadataContext? metadata)
        {
            if (sender is ItemsControl itemCollection)
            {
                Adapter.DiffableComparer = itemCollection.BindableMembers().DiffableEqualityComparer();
                return true;
            }

            return false;
        }

        void BindableCollectionAdapter.IHasItemChangedSupport.OnChanged(object? item, int index, object? args) => this[index] = item;
    }
}