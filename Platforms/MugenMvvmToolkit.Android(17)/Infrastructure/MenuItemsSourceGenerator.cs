#region Copyright
// ****************************************************************************
// <copyright file="MenuItemsSourceGenerator.cs">
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
using System.Collections;
using Android.Content;
using Android.Views;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Infrastructure
{
    internal sealed class MenuItemsSourceGenerator : ItemsSourceGeneratorBase
    {
        #region Fields

        private readonly Context _context;
        private readonly MenuItemTemplate _itemTemplate;
        private readonly IMenu _menu;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="MenuItemsSourceGenerator" /> class.
        /// </summary>
        public MenuItemsSourceGenerator(IMenu menu, Context context, MenuItemTemplate itemTemplate)
        {
            Should.NotBeNull(menu, "menu");
            Should.NotBeNull(itemTemplate, "itemTemplate");
            _menu = menu;
            _context = context;
            _itemTemplate = itemTemplate;
            TryListenActivity(context);
        }

        #endregion

        #region Overrides of ItemsSourceGeneratorBase

        protected override IEnumerable ItemsSource { get; set; }

        protected override void Add(int insertionIndex, int count)
        {
            if (insertionIndex == _menu.Size())
            {
                for (int i = 0; i < count; i++)
                {
                    int index = insertionIndex + i;
                    _itemTemplate.Apply(_menu, _context, GetItem(index), index, index);
                }
            }
            else
                Refresh();
        }

        protected override void Remove(int removalIndex, int count)
        {
            if (removalIndex == _menu.Size() - 1)
            {
                for (int i = 0; i < count; i++)
                    _menu.RemoveItem(removalIndex + i);
            }
            else
                Refresh();
        }

        protected override void Replace(int startIndex, int count)
        {
            for (int i = 0; i < count; i++)
            {
                int index = startIndex + i;
                _menu.RemoveItem(index);
                _itemTemplate.Apply(_menu, _context, GetItem(index), index, index);
            }
        }

        protected override void Refresh()
        {
            _menu.Clear();
            IEnumerable itemsSource = ItemsSource;
            if (itemsSource == null)
                return;
            int count = 0;
            foreach (object item in itemsSource)
            {
                _itemTemplate.Apply(_menu, _context, item, count, count);
                count++;
            }
        }

        #endregion
    }
}