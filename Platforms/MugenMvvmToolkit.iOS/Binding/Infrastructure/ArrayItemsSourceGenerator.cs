#region Copyright

// ****************************************************************************
// <copyright file="ArrayItemsSourceGenerator.cs">
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.iOS.Binding.Converters;
using ObjCRuntime;
using UIKit;

namespace MugenMvvmToolkit.iOS.Binding.Infrastructure
{
    public class ArrayItemsSourceGenerator<TContainer, TItem> : ItemsSourceGeneratorBase
        where TContainer : class
        where TItem : class
    {
        #region Fields

        private readonly WeakReference _containerRef;
        private readonly List<KeyValuePair<object, TItem>> _items;
        private readonly Action<TContainer, TItem[]> _setItems;
        private readonly IBindingMemberInfo _templateMemberInfo;
        private readonly bool _isControllerItem;

        #endregion

        #region Constructors

        public ArrayItemsSourceGenerator([NotNull] TContainer container, [NotNull] string templateMemberName,
            [NotNull] Action<TContainer, TItem[]> setItems)
        {
            Should.NotBeNull(container, nameof(container));
            Should.NotBeNull(templateMemberName, nameof(templateMemberName));
            Should.NotBeNull(setItems, nameof(setItems));
            _isControllerItem = typeof(UIViewController).IsAssignableFrom(typeof(TItem));
            _items = new List<KeyValuePair<object, TItem>>();
            _containerRef = TouchToolkitExtensions.CreateWeakReference(container);
            _setItems = setItems;
            _templateMemberInfo = BindingServiceProvider
                .MemberProvider
                .GetBindingMember(container.GetType(), templateMemberName, false, false);
            TryListenController(container as INativeObject);
        }

        #endregion

        #region Overrides of ItemsSourceGeneratorBase

        protected override IEnumerable ItemsSource { get; set; }

        protected override bool IsTargetDisposed => _containerRef.Target == null;

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

        protected override void OnTargetDisposed(object sender, EventArgs e)
        {
            base.OnTargetDisposed(sender, e);
            _items.Clear();
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
                (item as IViewModel)?.Settings.Metadata.AddOrUpdate(ViewModelConstants.StateNotNeeded, true);
                var uiView = template as UIView;
                if (uiView != null)
                {
                    template = new UIViewController { View = uiView };
                    if (item is IHasDisplayName)
                        BindingServiceProvider.BindingProvider.CreateBindingsFromString(template, "Title DisplayName");
                    template.SetDataContext(item);
                }
                TouchToolkitExtensions.SetHasState((UIViewController)template, false);
            }
            return new KeyValuePair<object, TItem>(item, (TItem)template);
        }

        private object GetItemFromTemplate(object item)
        {
            var target = _containerRef.Target;
            if (_templateMemberInfo == null || target == null)
                return GetDefaultTemplate(item);

            var selector = (IDataTemplateSelector)_templateMemberInfo.GetValue(target, null);
            if (selector == null)
                return GetDefaultTemplate(item);
            return selector.SelectTemplateWithContext(item, target);
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
            (item.Key as IViewModel)?.Settings.Metadata.Remove(ViewModelConstants.StateNotNeeded);
        }

        private void UpdateItems()
        {
            var container = (TContainer)_containerRef.Target;
            if (container == null)
            {
                OnTargetDisposed(null, EventArgs.Empty);
                return;
            }
            if (_items.Count == 0)
            {
                _setItems(container, Empty.Array<TItem>());
                return;
            }
            var items = new TItem[_items.Count];
            for (int i = 0; i < _items.Count; i++)
                items[i] = _items[i].Value;
            _setItems(container, items);
        }

        #endregion
    }
}
