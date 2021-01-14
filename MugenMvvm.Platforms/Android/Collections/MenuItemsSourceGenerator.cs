using System;
using Android.Views;
using MugenMvvm.Android.Constants;
using MugenMvvm.Android.Interfaces;
using MugenMvvm.Android.Members;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Collections;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Collections;

namespace MugenMvvm.Android.Collections
{
    public sealed class MenuItemsSourceGenerator : DiffableBindableCollectionAdapter
    {
        private MenuItemsSourceGenerator(IMenu menu, IMenuItemTemplate itemTemplate)
        {
            Should.NotBeNull(itemTemplate, nameof(itemTemplate));
            Menu = menu;
            ItemTemplate = itemTemplate;
            DiffableComparer = itemTemplate as IDiffableEqualityComparer;
        }

        public IMenu Menu { get; }

        public IMenuItemTemplate ItemTemplate { get; }

        protected override bool IsAlive => Menu.Handle != IntPtr.Zero;

        public static MenuItemsSourceGenerator? TryGet(IMenu menu)
        {
            menu.AttachedValues().TryGet(AndroidInternalConstant.MenuItemsSource, out var provider);
            return provider as MenuItemsSourceGenerator;
        }

        public static MenuItemsSourceGenerator GetOrAdd(IMenu menu)
        {
            Should.NotBeNull(menu, nameof(menu));
            return menu.AttachedValues().GetOrAdd(AndroidInternalConstant.MenuItemsSource, menu, (_, m) => new MenuItemsSourceGenerator(m, m.BindableMembers().ItemTemplate()!));
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

        protected override void OnClear(bool batchUpdate, int version)
        {
            base.OnClear(batchUpdate, version);
            Clear();
        }

        private void RemoveMenuItem(int id)
        {
            var menuItem = Menu.FindItem(id)!;
            ItemTemplate.Clear(menuItem);
            Menu.RemoveItem(id);
        }

        private void Reload()
        {
            Clear();
            for (var i = 0; i < Items.Count; i++)
                ItemTemplate.Apply(Menu, i, i, Items[i]);
        }

        private void Clear()
        {
            var size = Menu.Size();
            for (var i = 0; i < size; i++)
                ItemTemplate.Clear(Menu.GetItem(i)!);
            Menu.Clear();
        }
    }
}