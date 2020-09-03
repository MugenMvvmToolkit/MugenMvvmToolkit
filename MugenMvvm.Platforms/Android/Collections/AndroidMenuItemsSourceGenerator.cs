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
    public sealed class AndroidMenuItemsSourceGenerator : BindableCollectionAdapter
    {
        #region Constructors

        private AndroidMenuItemsSourceGenerator(IMenu menu, IMenuItemTemplate itemTemplate)
        {
            Should.NotBeNull(itemTemplate, nameof(itemTemplate));
            Menu = menu;
            ItemTemplate = itemTemplate;
        }

        #endregion

        #region Properties

        public IMenu Menu { get; }

        public IMenuItemTemplate ItemTemplate { get; }

        protected override bool IsAlive => Menu.Handle != IntPtr.Zero;

        #endregion

        #region Methods

        public static AndroidMenuItemsSourceGenerator? TryGet(IMenu view)
        {
            MugenService
                .AttachedValueManager
                .TryGetAttachedValues(view)
                .TryGet(AndroidInternalConstant.MenuItemsSource, out var provider);
            return provider as AndroidMenuItemsSourceGenerator;
        }

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
            if (index == Menu.Size())
                ItemTemplate.Apply(Menu, index, index, item);
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
            if (index == Menu.Size() - 1)
                RemoveMenuItem(index);
            else
                Reload();
        }

        protected override void OnReplaced(object? oldItem, object? newItem, int index, bool batchUpdate, int version)
        {
            base.OnReplaced(oldItem, newItem, index, batchUpdate, version);
            RemoveMenuItem(index);
            ItemTemplate.Apply(Menu, index, index, newItem);
        }

        protected override void OnReset(IEnumerable<object?>? items, bool batchUpdate, int version)
        {
            base.OnReset(items, batchUpdate, version);
            Reload();
        }

        private void RemoveMenuItem(int id)
        {
            var menuItem = Menu.FindItem(id);
            ItemTemplate.Clear(menuItem);
            Menu.RemoveItem(id);
        }

        private void Reload()
        {
            var size = Menu.Size();
            for (var i = 0; i < size; i++)
                ItemTemplate.Clear(Menu.GetItem(i));
            Menu.Clear();

            for (var i = 0; i < Items.Count; i++)
                ItemTemplate.Apply(Menu, i, i, Items[i]);
        }

        #endregion
    }
}