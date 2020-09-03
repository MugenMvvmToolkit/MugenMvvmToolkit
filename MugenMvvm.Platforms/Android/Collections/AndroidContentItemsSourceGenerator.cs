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
    public sealed class AndroidContentItemsSourceGenerator : BindableCollectionAdapter
    {
        #region Constructors

        private AndroidContentItemsSourceGenerator(View view, IContentTemplateSelector contentTemplateSelector)
        {
            View = view;
            ContentTemplateSelector = contentTemplateSelector;
        }

        #endregion

        #region Properties

        public View View { get; }

        public IContentTemplateSelector ContentTemplateSelector { get; }

        protected override bool IsAlive => View.Handle != IntPtr.Zero;

        #endregion

        #region Methods

        public static AndroidContentItemsSourceGenerator? TryGet(View view)
        {
            MugenService
                .AttachedValueManager
                .TryGetAttachedValues(view)
                .TryGet(AndroidInternalConstant.ItemsSourceGenerator, out var provider);
            return provider as AndroidContentItemsSourceGenerator;
        }

        public static AndroidContentItemsSourceGenerator GetOrAdd(View owner, IContentTemplateSelector selector)
        {
            Should.NotBeNull(owner, nameof(owner));
            Should.NotBeNull(selector, nameof(selector));
            return MugenService
                .AttachedValueManager
                .TryGetAttachedValues(owner)
                .GetOrAdd(AndroidInternalConstant.ItemsSourceGenerator, selector, (o, templateSelector) => new AndroidContentItemsSourceGenerator((View)o, templateSelector));
        }

        protected override void OnAdded(object? item, int index, bool batchUpdate, int version)
        {
            base.OnAdded(item, index, batchUpdate, version);
            ViewGroupExtensions.Add(View, GetItem(item)!, index, false);
        }

        protected override void OnMoved(object? item, int oldIndex, int newIndex, bool batchUpdate, int version)
        {
            base.OnMoved(item, oldIndex, newIndex, batchUpdate, version);
            var selected = ViewGroupExtensions.GetSelectedIndex(View) == oldIndex;
            if (TabLayoutExtensions.IsSupported(View))
            {
                ViewGroupExtensions.Remove(View, oldIndex);
                ViewGroupExtensions.Add(View, GetItem(item)!, newIndex, selected);
            }
            else
            {
                var target = ViewGroupExtensions.Get(View, oldIndex);
                ViewGroupExtensions.Remove(View, oldIndex);
                ViewGroupExtensions.Add(View, target, newIndex, selected);
            }
        }

        protected override void OnRemoved(object? item, int index, bool batchUpdate, int version)
        {
            base.OnRemoved(item, index, batchUpdate, version);
            ViewGroupExtensions.Remove(View, index);
        }

        protected override void OnReplaced(object? oldItem, object? newItem, int index, bool batchUpdate, int version)
        {
            base.OnReplaced(oldItem, newItem, index, batchUpdate, version);
            ViewGroupExtensions.Remove(View, index);
            ViewGroupExtensions.Add(View, GetItem(newItem)!, index, false);
        }

        protected override void OnReset(IEnumerable<object?>? items, bool batchUpdate, int version)
        {
            base.OnReset(items, batchUpdate, version);
            ViewGroupExtensions.Clear(View);
            for (var i = 0; i < Count; i++)
                ViewGroupExtensions.Add(View, GetItem(this[i])!, i, i == 0);
        }

        private Object? GetItem(object? item)
        {
            var template = (Object?)ContentTemplateSelector.SelectTemplate(View, item);
            if (template != null)
            {
                template.BindableMembers().SetDataContext(item);
                template.BindableMembers().SetParent(View);
            }

            return template;
        }

        #endregion
    }
}