#region Copyright
// ****************************************************************************
// <copyright file="MenuItemTemplate.cs">
// Copyright © Vyacheslav Volkov 2012-2014
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
using System.Xml.Serialization;
using Android.Content;
using Android.Views;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Infrastructure;

namespace MugenMvvmToolkit.Models
{
    public sealed class MenuItemTemplate
    {
        #region Fields

        private const string ActionViewBindKey = "@ActionViewBind";
        private const string ActionProviderBindKey = "@ActionProviderBind";

        #endregion

        #region Properties

        [XmlAttribute("DATACONTEXT")]
        public string DataContext { get; set; }

        [XmlAttribute("ALPHABETICSHORTCUT")]
        public string AlphabeticShortcut { get; set; }

        [XmlAttribute("ICON")]
        public string Icon { get; set; }

        [XmlAttribute("ISCHECKABLE")]
        public string IsCheckable { get; set; }

        [XmlAttribute("ISCHECKED")]
        public string IsChecked { get; set; }

        [XmlAttribute("ISENABLED")]
        public string IsEnabled { get; set; }

        [XmlAttribute("ISVISIBLE")]
        public string IsVisible { get; set; }

        [XmlAttribute("NUMERICSHORTCUT")]
        public string NumericShortcut { get; set; }

        [XmlAttribute("TITLE")]
        public string Title { get; set; }

        [XmlAttribute("TITLECONDENSED")]
        public string TitleCondensed { get; set; }

        [XmlAttribute("CLICK")]
        public string Click { get; set; }

        [XmlAttribute("COMMANDPARAMETER")]
        public string CommandParameter { get; set; }

        [XmlAttribute("ITEMSSOURCE")]
        public string ItemsSource { get; set; }

        [XmlAttribute("SHOWASACTION")]
        public string ShowAsAction { get; set; }

        [XmlAttribute("ISACTIONVIEWEXPANDED")]
        public string IsActionViewExpanded { get; set; }

        [XmlAttribute("ACTIONVIEW")]
        public string ActionView { get; set; }

        [XmlAttribute("ACTIONVIEWTEMPLATESELECTOR")]
        public string ActionViewTemplateSelector { get; set; }

        [XmlAttribute("ACTIONVIEWBIND")]
        public string ActionViewBind { get; set; }

        [XmlAttribute("ACTIONPROVIDER")]
        public string ActionProvider { get; set; }

        [XmlAttribute("ACTIONPROVIDERTEMPLATESELECTOR")]
        public string ActionProviderTemplateSelector { get; set; }

        [XmlAttribute("ACTIONPROVIDERBIND")]
        public string ActionProviderBind { get; set; }

        [XmlElement("ITEMTEMPLATE")]
        public MenuItemTemplate ItemTemplate { get; set; }

        [XmlElement("MENUITEM")]
        public List<MenuItemTemplate> Items { get; set; }

        #endregion

        #region Methods

        public static string GetActionViewBind(IMenuItem menuItem)
        {
            return ServiceProvider.AttachedValueProvider.GetValue<string>(menuItem, ActionViewBindKey, false);
        }

        public static string GetActionProviderBind(IMenuItem menuItem)
        {
            return ServiceProvider.AttachedValueProvider.GetValue<string>(menuItem, ActionProviderBindKey, false);
        }

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
                Clear(item, BindingServiceProvider.BindingManager);
            }
            catch (Exception e)
            {
                Tracer.Error(e.Flatten(true));
            }
        }

        internal static void Clear(IMenuItem item, IBindingManager bindingManager)
        {
            if (item == null)
                return;
            bindingManager.ClearBindings(item);
            if (item.HasSubMenu)
                MenuTemplate.Clear(item.SubMenu, bindingManager);
            AttachedMembersModule.MenuParentMember.SetValue(item, BindingExtensions.NullValue);
        }

        private void ApplyInternal(IMenu menu, Context context, int id, int order, object dataContext, bool useContext)
        {
            PlatformExtensions.ValidateTemplate(ItemsSource, Items);
            bool isSubMenu = !string.IsNullOrEmpty(ItemsSource) || (Items != null && Items.Count > 0);
            if (isSubMenu)
            {
                ISubMenu subMenu = menu.AddSubMenu(0, id, order, string.Empty);
                AttachedMembersModule.MenuParentMember.SetValue(subMenu, menu);
                AttachedMembersModule.MenuParentMember.SetValue(subMenu.Item, subMenu);
                SetDataContext(subMenu, context, dataContext, useContext);

                ApplySelf(subMenu.Item, context);
                if (string.IsNullOrEmpty(ItemsSource))
                {
                    for (int index = 0; index < Items.Count; index++)
                        Items[index].Apply(subMenu, context, index, index);
                }
                else
                {
                    MenuItemsSourceGenerator.Set(subMenu, context, ItemTemplate ?? this);
                    new XmlPropertySetter<MenuItemTemplate, ISubMenu>(subMenu, context)
                        .SetBinding(template => template.ItemsSource, ItemsSource, true);
                }
            }
            else
            {
                var menuItem = menu.Add(0, id, order, string.Empty);
                AttachedMembersModule.MenuParentMember.SetValue(menuItem, menu);
                SetDataContext(menuItem, context, dataContext, useContext);
                ApplySelf(menuItem, context);
            }
        }

        private void ApplySelf(IMenuItem menuItem, Context context)
        {
            var setter = new XmlPropertySetter<MenuItemTemplate, IMenuItem>(menuItem, context);
            setter.SetStringProperty(template => template.AlphabeticShortcut, AlphabeticShortcut);
            setter.SetStringProperty(template => template.NumericShortcut, NumericShortcut);
            setter.SetProperty(template => template.Icon, Icon);
            setter.SetBoolProperty(template => template.IsCheckable, IsCheckable);
            setter.SetBoolProperty(template => template.IsChecked, IsChecked);
            setter.SetBoolProperty(template => template.IsEnabled, IsEnabled);
            setter.SetBoolProperty(template => template.IsVisible, IsVisible);
            setter.SetBoolProperty(template => template.IsActionViewExpanded, IsActionViewExpanded);
            setter.SetStringProperty(template => template.Title, Title);
            setter.SetStringProperty(template => template.TitleCondensed, TitleCondensed);
            setter.SetStringProperty(template => template.CommandParameter, CommandParameter);
            setter.SetBinding(template => template.Click, Click, false);
#if !API8
            setter.SetEnumProperty<ShowAsAction>(template => template.ShowAsAction, ShowAsAction);

            if (!string.IsNullOrEmpty(ActionViewBind))
                ServiceProvider.AttachedValueProvider.SetValue(menuItem, ActionViewBindKey, ActionViewBind);
            if (!string.IsNullOrEmpty(ActionProviderBind))
                ServiceProvider.AttachedValueProvider.SetValue(menuItem, ActionProviderBindKey, ActionProviderBind);
#endif

            setter.SetBinding(template => template.ActionViewTemplateSelector, ActionViewTemplateSelector, false);
            setter.SetBinding(template => template.ActionProviderTemplateSelector, ActionProviderTemplateSelector, false);
            setter.SetProperty(template => template.ActionView, ActionView);
            setter.SetStringProperty(template => template.ActionProvider, ActionProvider);
#if API8SUPPORT
            menuItem.SetOnMenuItemClickListener(new AttachedMembersModule.MenuItemOnMenuItemClickListener(menuItem));
#else
            menuItem.SetOnMenuItemClickListener(AttachedMembersModule.MenuItemOnMenuItemClickListener.Instance);
#endif
        }

        private void SetDataContext<T>(T target, Context context, object dataContext, bool useContext)
        {
            if (useContext)
                BindingServiceProvider.ContextManager.GetBindingContext(target).Value = dataContext;
            else
                new XmlPropertySetter<MenuItemTemplate, T>(target, context)
                    .SetBinding(template => template.DataContext, DataContext, false);
        }

        #endregion
    }
}