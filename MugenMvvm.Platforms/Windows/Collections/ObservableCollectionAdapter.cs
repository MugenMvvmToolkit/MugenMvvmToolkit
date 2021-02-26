using System.Collections;
using System.Collections.ObjectModel;
using MugenMvvm.Collections;
#if AVALONIA
using Avalonia.Controls;

namespace MugenMvvm.Avalonia.Collections
#else
using System.Windows.Controls;

namespace MugenMvvm.Windows.Collections
#endif
{
    public sealed class ObservableCollectionAdapter : ObservableCollection<object?>
    {
        public ObservableCollectionAdapter()
        {
            Adapter = new DiffableBindableCollectionAdapter(this);
        }

        public DiffableBindableCollectionAdapter Adapter { get; }

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
    }
}