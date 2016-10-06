#region Copyright

// ****************************************************************************
// <copyright file="PlatformDataBindingModuleMenu.cs">
// Copyright (c) 2012-2016 Vyacheslav Volkov
// </copyright>
// ****************************************************************************
// <author>Vyacheslav Volkov</author>
// <email>vvs0205@outlook.com</email>
// <project>MugenMvvmToolkit</project>
// <web>https://github.com/MugenMvvmToolkit/MugenMvvmToolkit</web>
// <license>
// See license.txt in this solution or http://opensource.org/licenses/MS-PL
// </license>
// ****************************************************************************

#endregion

using System;
using System.Collections;
using Android.Graphics.Drawables;
using Android.Runtime;
using Android.Views;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Infrastructure;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Binding.Models.EventArg;
using Object = Java.Lang.Object;

namespace MugenMvvmToolkit.Android.Binding.Modules
{
    public partial class PlatformDataBindingModule
    {
        #region Nested types

        private sealed class MenuItemOnMenuItemClickListener : Object, IMenuItemOnMenuItemClickListener
        {
            #region Fields

            private const string Key = "#ClickListener";
            private readonly IMenuItem _item;

            #endregion

            #region Constructors

            public MenuItemOnMenuItemClickListener(IntPtr handle, JniHandleOwnership transfer) : base(handle, transfer)
            {
            }

            public MenuItemOnMenuItemClickListener(IMenuItem menuItem)
            {
                _item = menuItem;
            }

            #endregion

            #region Implementation of IMenuItemOnMenuItemClickListener

            public bool OnMenuItemClick(IMenuItem item)
            {
                if (_item == null)
                    return false;
                item = _item;
                if (item.IsCheckable)
                    item.SetBindingMemberValue(AttachedMembers.MenuItem.IsChecked, !item.IsChecked);
                EventListenerList.Raise(item, Key, EventArgs.Empty);
                return true;
            }

            #endregion

            #region Methods

            public static IDisposable AddClickListener(IMenuItem item, IEventListener listener)
            {
                return EventListenerList.GetOrAdd(item, Key).AddWithUnsubscriber(listener);
            }

            #endregion
        }

        #endregion

        #region Methods

        private static void RegisterMenuMembers(IBindingMemberProvider memberProvider)
        {
            //IMenu
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.Menu.ItemsSource, MenuItemsSourceChanged));
            var menuEnabledMember = AttachedBindingMember.CreateAutoProperty(AttachedMembers.Menu.Enabled,
                    (menu, args) => menu.SetGroupEnabled(0, args.NewValue.GetValueOrDefault()));
            memberProvider.Register(menuEnabledMember);
            memberProvider.Register("IsEnabled", menuEnabledMember);

            var menuVisibleMember = AttachedBindingMember.CreateAutoProperty(AttachedMembers.Menu.Visible,
                (menu, args) => menu.SetGroupVisible(0, args.NewValue.GetValueOrDefault()));
            memberProvider.Register(menuVisibleMember);
            memberProvider.Register("IsVisible", menuVisibleMember);

            //IMenuItem
            BindingBuilderExtensions.RegisterDefaultBindingMember(AttachedMembers.MenuItem.Click);
            memberProvider.Register(AttachedBindingMember.CreateNotifiableMember(AttachedMembers.MenuItem.IsChecked,
                (info, item) => item.IsChecked, (info, item, value) =>
                {
                    if (value == item.IsChecked)
                        return false;
                    item.SetChecked(value);
                    return true;
                }));
            memberProvider.Register(AttachedBindingMember.CreateEvent(AttachedMembers.MenuItem.Click, SetClickEventValue,
                (item, args) => item.SetOnMenuItemClickListener(new MenuItemOnMenuItemClickListener(item))));

            memberProvider.Register(AttachedBindingMember
                .CreateMember<IMenuItem, object>(nameof(IMenuItem.AlphabeticShortcut),
                    (info, item) => item.AlphabeticShortcut,
                    (info, item, value) =>
                    {
                        if (value is char)
                            item.SetAlphabeticShortcut((char)value);
                        else
                            item.SetAlphabeticShortcut(value.ToStringSafe()[0]);
                    }));
            memberProvider.Register(AttachedBindingMember
                .CreateMember(AttachedMembers.MenuItem.Icon, (info, item) => item.Icon,
                    (info, item, value) =>
                    {
                        if (value is int)
                            item.SetIcon((int)value);
                        else
                            item.SetIcon((Drawable)value);
                    }));

            memberProvider.Register(AttachedBindingMember
                .CreateMember<IMenuItem, bool>(nameof(IMenuItem.IsCheckable),
                    (info, item) => item.IsCheckable,
                    (info, item, value) => item.SetCheckable(value)));

            var menuItemEnabled = AttachedBindingMember.CreateMember<IMenuItem, bool>(AttachedMemberConstants.Enabled,
                (info, item) => item.IsEnabled,
                (info, item, value) => item.SetEnabled(value));
            memberProvider.Register(menuItemEnabled);
            memberProvider.Register(nameof(IMenuItem.IsEnabled), menuItemEnabled);
            memberProvider.Register(AttachedBindingMember
                .CreateMember<IMenuItem, bool>(nameof(IMenuItem.IsVisible), (info, item) => item.IsVisible,
                    (info, item, value) => item.SetVisible(value)));
            memberProvider.Register(AttachedBindingMember
                .CreateMember<IMenuItem, object>(nameof(IMenuItem.NumericShortcut),
                    (info, item) => item.NumericShortcut,
                    (info, item, value) =>
                    {
                        if (value is char)
                            item.SetNumericShortcut((char)value);
                        else
                            item.SetNumericShortcut(value.ToStringSafe()[0]);
                    }));
            memberProvider.Register(AttachedBindingMember.CreateMember(AttachedMembers.MenuItem.Title,
                (info, item) => item.TitleFormatted.ToStringSafe(),
                (info, item, value) => item.SetTitle(value)));
            memberProvider.Register(AttachedBindingMember.CreateMember(AttachedMembers.MenuItem.TitleCondensed,
                (info, item) => item.TitleCondensedFormatted.ToStringSafe(),
                (info, item, value) => item.SetTitleCondensed(value)));
        }

        private static void MenuItemsSourceChanged(IMenu menu, AttachedMemberChangedEventArgs<IEnumerable> args)
        {
            menu.GetBindingMemberValue(AttachedMembers.Menu.ItemsSourceGenerator)?.SetItemsSource(args.NewValue);
        }

        private static IDisposable SetClickEventValue(IBindingMemberInfo bindingMemberInfo, IMenuItem menuItem, IEventListener listener)
        {
            return MenuItemOnMenuItemClickListener.AddClickListener(menuItem, listener);
        }

        #endregion
    }
}
