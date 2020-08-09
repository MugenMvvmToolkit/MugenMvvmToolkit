using System;
using System.Collections.Generic;
using Android.Views;
using MugenMvvm.Android.Constants;
using MugenMvvm.Android.Interfaces;
using MugenMvvm.Android.Members;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Collections;

namespace MugenMvvm.Android.Collections
{
    public class AndroidMenuItemsSourceGenerator : BindableCollectionAdapter
    {
        #region Fields

        private readonly IMenu _menu;
        private readonly IMenuItemTemplate _template;

        #endregion

        #region Constructors

        private AndroidMenuItemsSourceGenerator(IMenu menu, IMenuItemTemplate template)
        {
            Should.NotBeNull(template, nameof(template));
            _menu = menu;
            _template = template;
        }

        #endregion

        #region Properties

        protected override bool IsAlive => _menu.Handle != IntPtr.Zero;

        #endregion

        #region Methods

        public static AndroidMenuItemsSourceGenerator GetOrAdd(IMenu menu)
        {
            Should.NotBeNull(menu, nameof(menu));
            return MugenService
                .AttachedValueManager
                .TryGetAttachedValues(menu)
                .GetOrAdd(AndroidInternalConstant.MenuItemsSource, menu, (_, m) => new AndroidMenuItemsSourceGenerator(m, m.BindableMembers().ItemTemplate()!));
        }

        protected override void OnAdded(object? item, int index, bool batchUpdate, int version)
        {
            base.OnAdded(item, index, batchUpdate, version);
            if (index == _menu.Size())
                _template.Apply(_menu, index, index, item);
            else
                Reload();
        }

        protected override void OnMoved(object? item, int oldIndex, int newIndex, bool batchUpdate, int version)
        {
            base.OnMoved(item, oldIndex, newIndex, batchUpdate, version);
            Reload();
        }

        protected override void OnRemoved(object? item, int index, bool batchUpdate, int version)
        {
            base.OnRemoved(item, index, batchUpdate, version);
            if (index == _menu.Size() - 1)
                RemoveMenuItem(index);
            else
                Reload();
        }

        protected override void OnReplaced(object? oldItem, object? newItem, int index, bool batchUpdate, int version)
        {
            base.OnReplaced(oldItem, newItem, index, batchUpdate, version);
            RemoveMenuItem(index);
            _template.Apply(_menu, index, index, newItem);
        }

        protected override void OnReset(IEnumerable<object?> items, bool batchUpdate, int version)
        {
            base.OnReset(items, batchUpdate, version);
            Reload();
        }

        protected override void OnCleared(bool batchUpdate, int version)
        {
            base.OnCleared(batchUpdate, version);
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