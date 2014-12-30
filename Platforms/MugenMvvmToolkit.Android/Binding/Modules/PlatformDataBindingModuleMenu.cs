#region Copyright

// ****************************************************************************
// <copyright file="PlatformDataBindingModuleMenu.cs">
// Copyright (c) 2012-2015 Vyacheslav Volkov
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
using Android.Views;
using MugenMvvmToolkit.Binding.Infrastructure;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Binding.Models.EventArg;
using Object = Java.Lang.Object;

namespace MugenMvvmToolkit.Binding.Modules
{
    public partial class PlatformDataBindingModule
    {
        #region Nested types

        internal sealed class MenuItemOnMenuItemClickListener : Object, IMenuItemOnMenuItemClickListener
        {
            #region Fields

            private const string Key = "#ClickListener";
            private readonly IMenuItem _item;

            #endregion

            #region Constructors

            public MenuItemOnMenuItemClickListener(IMenuItem menuItem)
            {
                _item = menuItem;
            }

            #endregion

            #region Implementation of IMenuItemOnMenuItemClickListener

            public bool OnMenuItemClick(IMenuItem item)
            {
                item = _item;
                if (item.IsCheckable)
                    IsCheckedMenuItemMember.SetValue(item, !item.IsChecked);
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

        #region Fields

        private static readonly IAttachedBindingMemberInfo<IMenuItem, bool> IsCheckedMenuItemMember;
        private static readonly IAttachedBindingMemberInfo<IMenu, IEnumerable> MenuItemsSourceMember;

        #endregion

        #region Methods

        private static void RegisterMenuMembers(IBindingMemberProvider memberProvider)
        {
            //IMenu
            memberProvider.Register(MenuItemsSourceMember);
            var menuEnabledMember = AttachedBindingMember.CreateAutoProperty<IMenu, bool?>(AttachedMemberConstants.Enabled,
                    (menu, args) => menu.SetGroupEnabled(0, args.NewValue.GetValueOrDefault()));
            memberProvider.Register(menuEnabledMember);
            memberProvider.Register("IsEnabled", menuEnabledMember);

            var menuVisibleMember = AttachedBindingMember.CreateAutoProperty<IMenu, bool?>("Visible",
                (menu, args) => menu.SetGroupVisible(0, args.NewValue.GetValueOrDefault()));
            memberProvider.Register(menuVisibleMember);
            memberProvider.Register("IsVisible", menuVisibleMember);

            //IMenuItem
            memberProvider.Register(IsCheckedMenuItemMember);
            memberProvider.Register(AttachedBindingMember.CreateEvent<IMenuItem>("Click", SetClickEventValue));

            memberProvider.Register(AttachedBindingMember
                .CreateMember<IMenuItem, object>("AlphabeticShortcut",
                    (info, item) => item.AlphabeticShortcut,
                    (info, item, value) =>
                    {
                        if (value is char)
                            item.SetAlphabeticShortcut((char)value);
                        else
                            item.SetAlphabeticShortcut(value.ToStringSafe()[0]);
                    }));
            memberProvider.Register(AttachedBindingMember
                .CreateMember<IMenuItem, object>("Icon", (info, item) => item.Icon,
                    (info, item, value) =>
                    {
                        if (value is int)
                            item.SetIcon((int)value);
                        else
                            item.SetIcon((Drawable)value);
                    }));

            memberProvider.Register(AttachedBindingMember
                .CreateMember<IMenuItem, bool>("IsCheckable",
                    (info, item) => item.IsCheckable,
                    (info, item, value) => item.SetCheckable(value)));

            var menuItemEnabled = AttachedBindingMember.CreateMember<IMenuItem, bool>(AttachedMemberConstants.Enabled,
                (info, item) => item.IsEnabled,
                (info, item, value) => item.SetEnabled(value));
            memberProvider.Register(menuItemEnabled);
            memberProvider.Register("IsEnabled", menuItemEnabled);
            memberProvider.Register(AttachedBindingMember
                .CreateMember<IMenuItem, bool>("IsVisible", (info, item) => item.IsVisible,
                    (info, item, value) => item.SetVisible(value)));
            memberProvider.Register(AttachedBindingMember
                .CreateMember<IMenuItem, object>("NumericShortcut",
                    (info, item) => item.NumericShortcut,
                    (info, item, value) =>
                    {
                        if (value is char)
                            item.SetNumericShortcut((char)value);
                        else
                            item.SetNumericShortcut(value.ToStringSafe()[0]);
                    }));
            memberProvider.Register(AttachedBindingMember.CreateMember<IMenuItem, string>("Title",
                (info, item) => item.TitleFormatted.ToString(),
                (info, item, value) => item.SetTitle(value)));
            memberProvider.Register(AttachedBindingMember.CreateMember<IMenuItem, string>("TitleCondensed",
                (info, item) => item.TitleCondensedFormatted.ToString(),
                (info, item, value) => item.SetTitleCondensed(value)));
        }

        private static void MenuItemsSourceChanged(IMenu menu, AttachedMemberChangedEventArgs<IEnumerable> args)
        {
            var generator = ItemsSourceGeneratorBase.Get(menu);
            if (generator != null)
                generator.SetItemsSource(args.NewValue);
        }

        private static IDisposable SetClickEventValue(IBindingMemberInfo bindingMemberInfo, IMenuItem menuItem, IEventListener listener)
        {
            return MenuItemOnMenuItemClickListener.AddClickListener(menuItem, listener);
        }

        #endregion
    }
}