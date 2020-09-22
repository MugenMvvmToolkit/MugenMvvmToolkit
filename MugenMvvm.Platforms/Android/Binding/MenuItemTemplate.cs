using System;
using Android.Views;
using Java.Lang;
using MugenMvvm.Android.Interfaces;
using MugenMvvm.Android.Members;
using MugenMvvm.Binding.Build;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Members;
using String = Java.Lang.String;

namespace MugenMvvm.Android.Binding
{
    public sealed class MenuItemTemplate : IMenuItemTemplate
    {
        #region Fields

        private static readonly ICharSequence EmptyString = new String("");

        #endregion

        #region Properties

        public Action<IMenuItem>? ApplyHandler { get; set; }

        public Action<IMenuItem>? ClearHandler { get; set; }

        public Func<IMenu, object?, MenuBindInfo>? Bind { get; set; }

        #endregion

        #region Implementation of interfaces

        public void Apply(IMenu menu, int id, int order, object? item)
        {
            var bindInfo = Bind?.Invoke(menu, item) ?? default;
            if (bindInfo.IsSubMenu)
            {
                var subMenu = menu.AddSubMenu(0, id, order, EmptyString)!;
                subMenu.BindableMembers().SetDataContext(item);
                subMenu.BindableMembers().SetParent(menu);
                if (bindInfo.ItemTemplate != null)
                    subMenu.BindableMembers().SetItemTemplate(bindInfo.ItemTemplate);
                if (bindInfo.SubMenuBind != null)
                    subMenu.Bind(bindInfo.SubMenuBind, includeResult: false);
                if (bindInfo.ItemBind != null)
                {
                    var menuItem = subMenu.Item!;
                    menuItem.BindableMembers().SetDataContext(item);
                    menuItem.BindableMembers().SetParent(menu);
                    menuItem.Bind(bindInfo.ItemBind, includeResult: false);
                }

                ApplyHandler?.Invoke(subMenu.Item!);
            }
            else
            {
                var menuItem = menu.Add(0, id, order, EmptyString)!;
                menuItem.BindableMembers().SetDataContext(item);
                menuItem.BindableMembers().SetParent(menu);
                if (bindInfo.ItemBind != null)
                    menuItem.Bind(bindInfo.ItemBind, includeResult: false);
                ApplyHandler?.Invoke(menuItem);
            }
        }

        public void Clear(IMenuItem menuItem)
        {
            ClearHandler?.Invoke(menuItem);
            ClearMenuItem(menuItem);
        }

        #endregion

        #region Methods

        public static void ClearMenuItem(IMenuItem? menuItem)
        {
            if (menuItem == null)
                return;
            if (menuItem.HasSubMenu)
                MenuTemplate.ClearMenu(menuItem.SubMenu);
            MugenBindingExtensions.ClearBindings(menuItem, true);
        }

        #endregion

        #region Nested types

        public readonly struct MenuBindInfo
        {
            #region Fields

            public readonly bool IsSubMenu;
            public readonly IMenuItemTemplate? ItemTemplate;
            public readonly string? ItemBind;
            public readonly string? SubMenuBind;

            #endregion

            #region Constructors

            private MenuBindInfo(IMenuItemTemplate? itemTemplate, string? subMenuBind, string? itemBind, bool isSubMenu)
            {
                SubMenuBind = subMenuBind;
                ItemBind = itemBind;
                ItemTemplate = itemTemplate;
                IsSubMenu = isSubMenu;
            }

            #endregion

            #region Methods

            public static MenuBindInfo Item(string? bind) => new MenuBindInfo(null, null, bind, false);

            public static MenuBindInfo SubMenu(IMenuItemTemplate? itemTemplate, string? subMenuBind, string? itemBind) => new MenuBindInfo(itemTemplate, subMenuBind, itemBind, true);

            #endregion
        }

        #endregion
    }
}