using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;
#if AVALONIA
using Avalonia.Controls;

namespace MugenMvvm.Avalonia.Collections
#else
using System.Windows.Controls;

namespace MugenMvvm.Windows.Collections
#endif
{
    public sealed class ObservableCollectionAdapter : ObservableCollection<object?>, ISuspendable
    {
        private bool _isNotificationsDirty;
        private int _suspendCount;

        public ObservableCollectionAdapter()
        {
            Adapter = new DiffableBindableCollectionAdapter(this);
        }

        public DiffableBindableCollectionAdapter Adapter { get; }

        public bool IsSuspended => _suspendCount != 0;

#if AVALONIA
        public static ObservableCollectionAdapter GetOrAdd(ItemsControl target)
        {
            if (target.Items is not ObservableCollectionAdapter c)
            {
                c = new ObservableCollectionAdapter();
                target.Items = c;
            }

            return c;
        }
#else
        public static ObservableCollectionAdapter GetOrAdd(ItemsControl target)
        {
            if (target.ItemsSource is not ObservableCollectionAdapter c)
            {
                c = new ObservableCollectionAdapter();
                target.ItemsSource = c;
            }

            return c;
        }
#endif

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
    }
}