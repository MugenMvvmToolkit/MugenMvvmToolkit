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
using System.Xml.Serialization;
using Android.Content;
using Android.Views;
using JetBrains.Annotations;
using MugenMvvmToolkit.Android.Binding.Infrastructure;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Builders;

namespace MugenMvvmToolkit.Android.Binding.Models
{
    public sealed class MenuItemTemplate
    {
        #region Properties

        [CanBeNull]
        public static Action<MenuItemTemplate, IMenuItem, XmlPropertySetter<IMenuItem>> Initalized;

        [XmlAttribute("BIND")]
        public string Bind { get; set; }

        [XmlAttribute("DATACONTEXT")]
        public string DataContext { get; set; }

        [XmlAttribute("GROUP")]
        public string Group { get; set; }

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
            bool isSubMenu = !string.IsNullOrEmpty(ItemsSource) || (Items != null && Items.Count > 0);
            XmlPropertySetter<IMenuItem> setter;
            int groupId;
            int.TryParse(Group, out groupId);
            if (isSubMenu)
            {
                ISubMenu subMenu = menu.AddSubMenu(groupId, id, order, string.Empty);
                setter = new XmlPropertySetter<IMenuItem>(subMenu.Item, context, new BindingSet());
                subMenu.SetBindingMemberValue(AttachedMembers.Object.Parent, menu);
                subMenu.Item.SetBindingMemberValue(AttachedMembers.Object.Parent, subMenu);
                SetDataContext(subMenu, setter.BindingSet, dataContext, useContext);
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
                    XmlPropertySetter<object>.AddBinding(setter.BindingSet, subMenu, AttachedMemberConstants.ItemsSource, ItemsSource, true);
                }
            }
            else
            {
                var menuItem = menu.Add(groupId, id, order, string.Empty);
                setter = new XmlPropertySetter<IMenuItem>(menuItem, context, new BindingSet());
                menuItem.SetBindingMemberValue(AttachedMembers.Object.Parent, menu);
                SetDataContext(menuItem, setter.BindingSet, dataContext, useContext);
                ApplySelf(menuItem, setter);
            }
            setter.Apply();
        }

        private void ApplySelf(IMenuItem menuItem, XmlPropertySetter<IMenuItem> setter)
        {
            if (!string.IsNullOrEmpty(Bind))
                setter.BindingSet.BindFromExpression(menuItem, Bind);
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

            var initalized = Initalized;
            if (initalized != null)
                initalized(this, menuItem, setter);

            setter.SetBinding(nameof(ActionViewTemplateSelector), ActionViewTemplateSelector, false);
            setter.SetBinding(nameof(ActionProviderTemplateSelector), ActionProviderTemplateSelector, false);
            setter.SetProperty(nameof(ActionView), ActionView);
            setter.SetStringProperty(nameof(ActionProvider), ActionProvider);
        }

        private void SetDataContext(object target, BindingSet setter, object dataContext, bool useContext)
        {
            if (useContext)
                target.SetDataContext(dataContext);
            else
                XmlPropertySetter<object>.AddBinding(setter, target, AttachedMemberConstants.DataContext, DataContext, false);
        }

        #endregion
    }
}
