#region Copyright

// ****************************************************************************
// <copyright file="MenuItemTemplate.cs">
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
using System.Collections.Generic;
using Android.Content;
using Android.Views;
using MugenMvvmToolkit.Android.Binding.Infrastructure;
using MugenMvvmToolkit.Binding;

namespace MugenMvvmToolkit.Android.Binding.Models
{
    public sealed class MenuItemTemplate
    {
        #region Properties

        public string Bind { get; set; }

        public string DataContext { get; set; }

        public string Group { get; set; }

        public string AlphabeticShortcut { get; set; }

        public string Icon { get; set; }

        public string IsCheckable { get; set; }

        public string IsChecked { get; set; }

        public string IsEnabled { get; set; }

        public string IsVisible { get; set; }

        public string NumericShortcut { get; set; }

        public string Title { get; set; }

        public string TitleCondensed { get; set; }

        public string Click { get; set; }

        public string CommandParameter { get; set; }

        public string ItemsSource { get; set; }

        public string ShowAsAction { get; set; }

        public string IsActionViewExpanded { get; set; }

        public string ActionView { get; set; }

        public object ActionViewTemplateSelector { get; set; }

        public string ActionViewBind { get; set; }

        public string ActionProvider { get; set; }

        public object ActionProviderTemplateSelector { get; set; }

        public string ActionProviderBind { get; set; }

        public MenuItemTemplate ItemTemplate { get; set; }

        public List<MenuItemTemplate> Items { get; set; }

        #endregion

        #region Methods

        public void Apply(IMenu menu, Context context, object dataContext, int id, int order)
        {
            ApplyInternal(menu, context, id, order, dataContext, true);
        }

        public void Apply(IMenu menu, Context context, int id, int order)
        {
            ApplyInternal(menu, context, id, order, null, false);
        }

        public static void Clear(IMenuItem item)
        {
            try
            {
                ClearInternal(item);
            }
            catch (Exception e)
            {
                Tracer.Error(e.Flatten(true));
            }
        }

        internal static void ClearInternal(IMenuItem item)
        {
            if (item == null)
                return;
            if (item.HasSubMenu)
                MenuTemplate.ClearInternal(item.SubMenu);
            item.ClearBindings(true, true);
        }

        private void ApplyInternal(IMenu menu, Context context, int id, int order, object dataContext, bool useContext)
        {
            PlatformExtensions.ValidateTemplate(ItemsSource, Items);
            bool isSubMenu = !string.IsNullOrEmpty(ItemsSource) || Items != null && Items.Count > 0;
            XmlPropertySetter setter;
            int groupId;
            int.TryParse(Group, out groupId);
            if (isSubMenu)
            {
                ISubMenu subMenu = menu.AddSubMenu(groupId, id, order, string.Empty);
                setter = new XmlPropertySetter(subMenu.Item, context);
                subMenu.SetBindingMemberValue(AttachedMembers.Object.Parent, menu);
                subMenu.Item.SetBindingMemberValue(AttachedMembers.Object.Parent, subMenu);
                SetDataContext(subMenu, setter, dataContext, useContext);
                ApplySelf(subMenu.Item, setter);

                if (string.IsNullOrEmpty(ItemsSource))
                {
                    for (int index = 0; index < Items.Count; index++)
                        Items[index].Apply(subMenu, context, index, index);
                }
                else
                {
                    subMenu.SetBindingMemberValue(AttachedMembers.Menu.ItemsSourceGenerator,
                        new MenuItemsSourceGenerator(subMenu, context, ItemTemplate ?? this));
                    XmlPropertySetter.AddBinding(setter, subMenu, AttachedMemberConstants.ItemsSource, ItemsSource, true);
                }
            }
            else
            {
                var menuItem = menu.Add(groupId, id, order, string.Empty);
                setter = new XmlPropertySetter(menuItem, context);
                menuItem.SetBindingMemberValue(AttachedMembers.Object.Parent, menu);
                SetDataContext(menuItem, setter, dataContext, useContext);
                ApplySelf(menuItem, setter);
            }
            setter.Apply();
        }

        private void ApplySelf(IMenuItem menuItem, XmlPropertySetter setter)
        {
            if (!string.IsNullOrEmpty(Bind))
                setter.Bind(menuItem, Bind);
            setter.SetStringProperty(nameof(AlphabeticShortcut), AlphabeticShortcut);
            setter.SetStringProperty(nameof(NumericShortcut), NumericShortcut);
            setter.SetProperty(nameof(Icon), Icon);
            setter.SetBoolProperty(nameof(IsCheckable), IsCheckable);
            setter.SetBoolProperty(nameof(IsChecked), IsChecked);
            setter.SetBoolProperty(nameof(IsEnabled), IsEnabled);
            setter.SetBoolProperty(nameof(IsVisible), IsVisible);
            setter.SetBoolProperty(nameof(IsActionViewExpanded), IsActionViewExpanded);
            setter.SetStringProperty(nameof(Title), Title);
            setter.SetStringProperty(nameof(TitleCondensed), TitleCondensed);
            setter.SetStringProperty(nameof(CommandParameter), CommandParameter);
            setter.SetBinding(nameof(Click), Click, false);

            PlatformExtensions.MenuItemTemplateInitalized?.Invoke(this, menuItem, setter);

            setter.SetProperty(nameof(ActionViewTemplateSelector), ActionViewTemplateSelector);
            setter.SetProperty(nameof(ActionProviderTemplateSelector), ActionProviderTemplateSelector);
            setter.SetProperty(nameof(ActionView), ActionView);
            setter.SetStringProperty(nameof(ActionProvider), ActionProvider);
        }

        private void SetDataContext(object target, XmlPropertySetter setter, object dataContext, bool useContext)
        {
            if (useContext)
                target.SetDataContext(dataContext);
            else
                XmlPropertySetter.AddBinding(setter, target, AttachedMemberConstants.DataContext, DataContext, false);
        }

        #endregion
    }
}
