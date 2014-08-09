#region Copyright
// ****************************************************************************
// <copyright file="MenuTemplate.cs">
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
using System.Collections.Generic;
using System.Xml.Serialization;
using Android.Content;
using Android.Views;
using MugenMvvmToolkit.Infrastructure;

namespace MugenMvvmToolkit.Models
{
    [XmlRoot("MENU", Namespace = "")]
    public sealed class MenuTemplate
    {
        #region Properties

        [XmlAttribute("ISENABLED")]
        public string IsEnabled { get; set; }

        [XmlAttribute("ISVISIBLE")]
        public string IsVisible { get; set; }

        [XmlAttribute("DATACONTEXT")]
        public string DataContext { get; set; }

        [XmlAttribute("ITEMSSOURCE")]
        public string ItemsSource { get; set; }

        [XmlElement("ITEMTEMPLATE")]
        public MenuItemTemplate ItemTemplate { get; set; }

        [XmlElement("MENUITEM")]
        public List<MenuItemTemplate> Items { get; set; }

        #endregion

        #region Methods

        public void Apply(IMenu menu, Context context, object parent)
        {
            PlatformExtensions.ValidateTemplate(ItemsSource, Items);
            var setter = new XmlPropertySetter<MenuTemplate, IMenu>(menu, context);
            AttachedMembersModule.MenuParentMember.SetValue(menu, new[] { parent });
            setter.SetBinding(template => template.DataContext, DataContext, false);
            setter.SetBoolProperty(template => template.IsVisible, IsVisible);
            setter.SetBoolProperty(template => template.IsEnabled, IsEnabled);
            if (string.IsNullOrEmpty(ItemsSource))
            {
                if (Items == null)
                    return;
                for (int index = 0; index < Items.Count; index++)
                    Items[index].Apply(menu, context, index, index);
            }
            else
            {
                AttachedMembersModule
                    .MenuItemsSourceGeneratorMember
                    .SetValue(menu, new object[] { new MenuItemsSourceGenerator(menu, context, ItemTemplate) });
                setter.SetBinding(template => template.ItemsSource, ItemsSource, true);
            }
        }

        #endregion
    }
}