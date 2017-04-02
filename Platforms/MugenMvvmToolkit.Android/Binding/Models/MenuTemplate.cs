#region Copyright

// ****************************************************************************
// <copyright file="MenuTemplate.cs">
// Copyright (c) 2012-2017 Vyacheslav Volkov
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
using MugenMvvmToolkit.Android.Binding.Interfaces;
using MugenMvvmToolkit.Binding;

namespace MugenMvvmToolkit.Android.Binding.Models
{
    public sealed class MenuTemplate : IMenuTemplate
    {
        #region Properties

        public string DataContext { get; set; }

        public string Bind { get; set; }

        public string IsEnabled { get; set; }

        public string IsVisible { get; set; }

        public string ItemsSource { get; set; }

        public MenuItemTemplate ItemTemplate { get; set; }

        public List<MenuItemTemplate> Items { get; set; }

        #endregion

        #region Methods

        public void Apply(IMenu menu, Context context, object parent)
        {
            AndroidToolkitExtensions.ValidateTemplate(ItemsSource, Items);
            var setter = new XmlPropertySetter(menu, context);
            menu.SetBindingMemberValue(AttachedMembers.Object.Parent, parent);
            setter.SetBinding(nameof(DataContext), DataContext, false);
            setter.SetBoolProperty(nameof(IsVisible), IsVisible);
            setter.SetBoolProperty(nameof(IsEnabled), IsEnabled);
            if (!string.IsNullOrEmpty(Bind))
                setter.Bind(menu, Bind);
            if (string.IsNullOrEmpty(ItemsSource))
            {
                if (Items != null)
                {
                    for (int index = 0; index < Items.Count; index++)
                        Items[index].Apply(menu, context, index, index);
                }
            }
            else
            {
                menu.SetBindingMemberValue(AttachedMembers.Menu.ItemsSourceGenerator, new MenuItemsSourceGenerator(menu, context, ItemTemplate));
                setter.SetBinding(nameof(ItemsSource), ItemsSource, true);
            }
            setter.Apply();
        }

        public static void Clear(IMenu menu)
        {
            try
            {
                ClearInternal(menu);
            }
            catch (Exception e)
            {
                Tracer.Error(e.Flatten(true));
            }
        }

        internal static void ClearInternal(IMenu menu)
        {
            if (menu == null)
                return;
            int size = menu.Size();
            for (int i = 0; i < size; i++)
                MenuItemTemplate.ClearInternal(menu.GetItem(i));
            menu.Clear();
            menu.ClearBindings(true, true);
        }

        #endregion
    }
}
