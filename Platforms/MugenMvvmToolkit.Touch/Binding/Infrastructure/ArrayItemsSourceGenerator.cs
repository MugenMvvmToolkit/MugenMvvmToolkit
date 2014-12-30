#region Copyright

// ****************************************************************************
// <copyright file="ArrayItemsSourceGenerator.cs">
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
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using MonoTouch.ObjCRuntime;
using MonoTouch.UIKit;
using MugenMvvmToolkit.Binding.Converters;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;

namespace MugenMvvmToolkit.Binding.Infrastructure
{
    public class ArrayItemsSourceGenerator<TContainer, TItem> : ItemsSourceGeneratorBase
        where TContainer : class
        where TItem : class
    {
        #region Fields

        private readonly TContainer _container;
        private readonly List<KeyValuePair<object, TItem>> _items;
        private readonly Action<TContainer, TItem[]> _setItems;
        private readonly IBindingMemberInfo _templateMemberInfo;
        private readonly bool _isControllerItem;

        #endregion

        #region Constructors

        public ArrayItemsSourceGenerator([NotNull] TContainer container, [NotNull] string templateMemberName,
            [NotNull] Action<TContainer, TItem[]> setItems)
        {
            Should.NotBeNull(container, "container");
            Should.NotBeNull(templateMemberName, "templateMemberName");
            Should.NotBeNull(setItems, "setItems");
            _isControllerItem = typeof(UIViewController).IsAssignableFrom(typeof(TItem));
            _items = new List<KeyValuePair<object, TItem>>();
            _container = container;
            _setItems = setItems;
            _templateMemberInfo = BindingServiceProvider
                .MemberProvider
                .GetBindingMember(container.GetType(), templateMemberName, false, false);
        }

        #endregion

        #region Overrides of ItemsSourceGeneratorBase

        protected override IEnumerable ItemsSource { get; set; }

        protected override bool IsTargetDisposed
        {
            get
            {
                var nativeObject = _container as INativeObject;
                if (nativeObject == null)
                    return false;
                return !nativeObject.IsAlive();
            }
        }

        protected override void Add(int insertionIndex, int count)
        {
            for (int i = 0; i < count; i++)
            {
                int index = insertionIndex + i;
                _items.Insert(index, GetItemFromTemplate(index));
            }
            UpdateItems();
        }

        protected override void Remove(int removalIndex, int count)
        {
            for (int i = 0; i < count; i++)
                RemoveItem(removalIndex + i);
            UpdateItems();
        }

        protected override void Replace(int startIndex, int count)
        {
            for (int i = 0; i < count; i++)
            {
                int index = startIndex + i;
                RemoveItem(index);
                _items.Insert(index, GetItemFromTemplate(index));
            }
            UpdateItems();
        }

        protected override void Refresh()
        {
            Dictionary<object, List<KeyValuePair<object, TItem>>> oldItems = null;
            if (_items.Count != 0)
            {
                oldItems = _items
                    .GroupBy(pair => pair.Key)
                    .ToDictionary(pairs => pairs.Key, pairs => pairs.ToList());
                _items.Clear();
            }
            int count = ItemsSource.Count();
            for (int i = 0; i < count; i++)
                _items.Add(GetItemFromTemplate(i, oldItems));

            if (oldItems != null)
            {
                foreach (var oldItem in oldItems.Values.SelectMany(list => list))
                    ClearItem(oldItem);
            }
            UpdateItems();
        }

        #endregion

        #region Methods

        private KeyValuePair<object, TItem> GetItemFromTemplate(int index,
            Dictionary<object, List<KeyValuePair<object, TItem>>> oldItems = null)
        {
            object item = GetItem(index);
            if (oldItems != null)
            {
                List<KeyValuePair<object, TItem>> list;
                if (oldItems.TryGetValue(item, out list))
                {
                    KeyValuePair<object, TItem> value = list[0];
                    list.RemoveAt(0);
                    if (list.Count == 0)
                        oldItems.Remove(item);
                    return value;
                }
            }
            var template = GetItemFromTemplate(item);
            if (_isControllerItem)
            {
                var viewModel = item as IViewModel;
                if (viewModel != null)
                    viewModel.Settings.Metadata.AddOrUpdate(ViewModelConstants.StateNotNeeded, true);
                var uiView = template as UIView;
                if (uiView != null)
                {
                    template = new UIViewController { View = uiView };
                    if (item is IHasDisplayName)
                        BindingServiceProvider.BindingProvider.CreateBindingsFromString(template, "Title DisplayName");
                    ViewManager.SetDataContext(template, item);
                }
                var controller = (UIViewController)template;
                if (controller.RestorationIdentifier != null)
                    controller.RestorationIdentifier = string.Empty;
            }
            return new KeyValuePair<object, TItem>(item, (TItem)template);
        }

        private object GetItemFromTemplate(object item)
        {
            if (_templateMemberInfo == null)
                return GetDefaultTemplate(item);
            var selector = (IDataTemplateSelector)_templateMemberInfo.GetValue(_container, null);
            if (selector == null)
                return GetDefaultTemplate(item);
            return selector.SelectTemplateWithContext(item, _container);
        }

        private static object GetDefaultTemplate(object item)
        {
            if (item is IViewModel)
                return ViewModelToViewConverter.Instance.Convert(item);
            return item;
        }

        private void RemoveItem(int index)
        {
            ClearItem(_items[index]);
            _items.RemoveAt(index);
        }

        private void ClearItem(KeyValuePair<object, TItem> item)
        {
            if (!_isControllerItem)
                return;
            var viewModel = item.Key as IViewModel;
            if (viewModel != null)
                viewModel.Settings.Metadata.Remove(ViewModelConstants.StateNotNeeded);
        }

        private void UpdateItems()
        {
            if (_items.Count == 0)
            {
                _setItems(_container, Empty.Array<TItem>());
                return;
            }
            var items = new TItem[_items.Count];
            for (int i = 0; i < _items.Count; i++)
                items[i] = _items[i].Value;
            _setItems(_container, items);
        }

        #endregion
    }
}