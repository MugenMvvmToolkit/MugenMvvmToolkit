using System;
using System.Collections.Generic;
using Android.Views;
using MugenMvvm.Android.Constants;
using MugenMvvm.Android.Interfaces;
using MugenMvvm.Android.Native.Views;
using MugenMvvm.Android.Native.Views.Support;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Members;
using MugenMvvm.Collections;
using Object = Java.Lang.Object;

namespace MugenMvvm.Android.Collections
{
    public sealed class AndroidContentItemsSourceGenerator : BindableCollectionAdapterBase<object?>
    {
        #region Fields

        private readonly View _owner;
        private readonly IContentTemplateSelector _selector;

        #endregion

        #region Constructors

        private AndroidContentItemsSourceGenerator(View owner, IContentTemplateSelector selector)
        {
            _owner = owner;
            _selector = selector;
        }

        #endregion

        #region Properties

        protected override bool IsAlive => _owner.Handle != IntPtr.Zero;

        #endregion

        #region Methods

        public static AndroidContentItemsSourceGenerator GetOrAdd(View owner, IContentTemplateSelector selector)
        {
            Should.NotBeNull(owner, nameof(owner));
            Should.NotBeNull(selector, nameof(selector));
            return MugenService
                .AttachedValueManager
                .TryGetAttachedValues(owner)
                .GetOrAdd(AndroidInternalConstant.ItemsSourceGenerator, selector, (o, templateSelector) => new AndroidContentItemsSourceGenerator((View) o, templateSelector));
        }

        protected override void OnAdded(object? item, int index, bool batch)
        {
            base.OnAdded(item, index, batch);
            ViewGroupExtensions.Add(_owner, GetItem(item)!, index, false);
        }

        protected override void OnCleared(bool batch)
        {
            base.OnCleared(batch);
            ViewGroupExtensions.Clear(_owner);
        }

        protected override void OnMoved(object? item, int oldIndex, int newIndex, bool batch)
        {
            base.OnMoved(item, oldIndex, newIndex, batch);
            var selected = ViewGroupExtensions.GetSelectedIndex(_owner) == oldIndex;
            if (TabLayoutExtensions.IsSupported(_owner))
            {
                ViewGroupExtensions.Remove(_owner, oldIndex);
                ViewGroupExtensions.Add(_owner, GetItem(item)!, newIndex, selected);
            }
            else
            {
                var target = ViewGroupExtensions.Get(_owner, oldIndex);
                ViewGroupExtensions.Remove(_owner, oldIndex);
                ViewGroupExtensions.Add(_owner, target, newIndex, selected);
            }
        }

        protected override void OnRemoved(object? item, int index, bool batch)
        {
            base.OnRemoved(item, index, batch);
            ViewGroupExtensions.Remove(_owner, index);
        }

        protected override void OnReplaced(object? oldItem, object? newItem, int index, bool batch)
        {
            base.OnReplaced(oldItem, newItem, index, batch);
            ViewGroupExtensions.Remove(_owner, index);
            ViewGroupExtensions.Add(_owner, GetItem(newItem)!, index, false);
        }

        protected override void OnReset(IEnumerable<object?> items, bool batch)
        {
            base.OnReset(items, batch);
            ViewGroupExtensions.Clear(_owner);
            for (var i = 0; i < Count; i++)
                ViewGroupExtensions.Add(_owner, GetItem(this[i])!, i, i == 0);
        }

        private Object? GetItem(object? item)
        {
            var template = (Object?) _selector.SelectTemplate(_owner, item);
            if (template != null)
            {
                template.BindableMembers().SetDataContext(item);
                template.BindableMembers().SetParent(_owner);
            }

            return template;
        }

        #endregion
    }
}