using System.Collections.Generic;
using Android.Views;
using MugenMvvm.Android.Constants;
using MugenMvvm.Android.Interfaces;
using MugenMvvm.Collections;

namespace MugenMvvm.Android.Collections
{
    public sealed class AndroidMenuItemsSourceAdapter : BindableCollectionAdapterBase<object?>
    {
        #region Fields

        private readonly IMenu _menu;
        private readonly IMenuItemTemplate _template;

        #endregion

        #region Constructors

        private AndroidMenuItemsSourceAdapter(IMenu menu, IMenuItemTemplate template)
        {
            _menu = menu;
            _template = template;
        }

        #endregion

        #region Methods

        public static AndroidMenuItemsSourceAdapter GetOrAdd(IMenu menu, IMenuItemTemplate template)
        {
            Should.NotBeNull(menu, nameof(menu));
            Should.NotBeNull(template, nameof(template));
            return MugenService.AttachedValueManager.GetOrAdd(menu, AndroidInternalConstant.MenuItemsSource, template, (m, t) => new AndroidMenuItemsSourceAdapter(m, t));
        }

        protected override void OnAdded(object? item, int index, bool batch)
        {
            base.OnAdded(item, index, batch);
            if (index == _menu.Size())
                _template.Apply(_menu, index, index, item);
            else
                Reload();
        }

        protected override void OnMoved(object? item, int oldIndex, int newIndex, bool batch)
        {
            base.OnMoved(item, oldIndex, newIndex, batch);
            Reload();
        }

        protected override void OnRemoved(object? item, int index, bool batch)
        {
            base.OnRemoved(item, index, batch);
            if (index == _menu.Size() - 1)
                RemoveMenuItem(index);
            else
                Reload();
        }

        protected override void OnReplaced(object? oldItem, object? newItem, int index, bool batch)
        {
            base.OnReplaced(oldItem, newItem, index, batch);
            RemoveMenuItem(index);
            _template.Apply(_menu, index, index, newItem);
        }

        protected override void OnReset(IEnumerable<object?> items, bool batch)
        {
            base.OnReset(items, batch);
            Reload();
        }

        protected override void OnCleared(bool batch)
        {
            base.OnCleared(batch);
            Reload();
        }

        private void RemoveMenuItem(int id)
        {
            var menuItem = _menu.FindItem(id);
            _template.Clear(menuItem);
            _menu.RemoveItem(id);
        }

        private void Reload()
        {
            var size = _menu.Size();
            for (var i = 0; i < size; i++)
                _template.Clear(_menu.GetItem(i));
            _menu.Clear();

            for (var i = 0; i < Items.Count; i++)
                _template.Apply(_menu, i, i, Items[i]);
        }

        #endregion
    }
}