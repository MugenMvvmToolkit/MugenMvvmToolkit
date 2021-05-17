using System;
using Android.Views;
using MugenMvvm.Android.Constants;
using MugenMvvm.Android.Interfaces;
using MugenMvvm.Android.Native.Views;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Members;
using MugenMvvm.Collections;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Collections;
using Object = Java.Lang.Object;

namespace MugenMvvm.Android.Collections
{
    public sealed class ContentItemsSourceGenerator : DiffableBindableCollectionAdapter
    {
        private readonly bool _isRecycleSupported;

        private ContentItemsSourceGenerator(View view, IContentTemplateSelector contentTemplateSelector)
        {
            View = view;
            ContentTemplateSelector = contentTemplateSelector;
            DiffableComparer = contentTemplateSelector as IDiffableEqualityComparer;
            _isRecycleSupported = NativeBindableMemberMugenExtensions.IsChildRecycleSupported(view);
        }

        public View View { get; }

        public IContentTemplateSelector ContentTemplateSelector { get; }

        protected override bool IsAlive => View.Handle != IntPtr.Zero;

        public static ContentItemsSourceGenerator? TryGet(View view)
        {
            view.AttachedValues().TryGet(AndroidInternalConstant.ItemsSourceGenerator, out var provider);
            return provider as ContentItemsSourceGenerator;
        }

        public static ContentItemsSourceGenerator GetOrAdd(View view, IContentTemplateSelector selector)
        {
            Should.NotBeNull(view, nameof(view));
            Should.NotBeNull(selector, nameof(selector));
            return view.AttachedValues().GetOrAdd(AndroidInternalConstant.ItemsSourceGenerator, selector,
                (o, templateSelector) => new ContentItemsSourceGenerator((View) o, templateSelector));
        }

        protected override bool IsChangeEventSupported(object? item, object? args) => false;

        protected override void OnAdded(object? item, int index, bool batchUpdate, int version)
        {
            base.OnAdded(item, index, batchUpdate, version);
            NativeBindableMemberMugenExtensions.AddChild(View, GetItem(item)!, index, false);
        }

        protected override void OnMoved(object? item, int oldIndex, int newIndex, bool batchUpdate, int version)
        {
            base.OnMoved(item, oldIndex, newIndex, batchUpdate, version);
            var selected = NativeBindableMemberMugenExtensions.GetSelectedIndex(View) == oldIndex;
            if (_isRecycleSupported)
            {
                var target = NativeBindableMemberMugenExtensions.GetChildAt(View, oldIndex);
                NativeBindableMemberMugenExtensions.RemoveChildAt(View, oldIndex);
                NativeBindableMemberMugenExtensions.AddChild(View, target, newIndex, selected);
            }
            else
            {
                NativeBindableMemberMugenExtensions.RemoveChildAt(View, oldIndex);
                NativeBindableMemberMugenExtensions.AddChild(View, GetItem(item)!, newIndex, selected);
            }
        }

        protected override void OnRemoved(object? item, int index, bool batchUpdate, int version)
        {
            base.OnRemoved(item, index, batchUpdate, version);
            NativeBindableMemberMugenExtensions.RemoveChildAt(View, index);
        }

        protected override void OnReplaced(object? oldItem, object? newItem, int index, bool batchUpdate, int version)
        {
            base.OnReplaced(oldItem, newItem, index, batchUpdate, version);
            NativeBindableMemberMugenExtensions.RemoveChildAt(View, index);
            NativeBindableMemberMugenExtensions.AddChild(View, GetItem(newItem)!, index, false);
        }

        protected override void OnClear(bool batchUpdate, int version)
        {
            base.OnClear(batchUpdate, version);
            NativeBindableMemberMugenExtensions.Clear(View);
        }

        private Object? GetItem(object? item)
        {
            var template = (Object?) ContentTemplateSelector.SelectTemplate(View, item);
            if (template != null)
            {
                template.BindableMembers().SetDataContext(item);
                template.BindableMembers().SetParent(View);
            }

            return template;
        }
    }
}