﻿using System;
using System.Runtime.CompilerServices;
using MugenMvvm.Bindings.Attributes;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Bindings.Members.Descriptors;
using MugenMvvm.Internal;
using MugenMvvm.Ios.Interfaces;
using UIKit;

namespace MugenMvvm.Ios.Bindings
{
    public static class IosBindableMembers
    {
        public static BindablePropertyDescriptor<T, bool> Visible<T>(this BindableMembersDescriptor<T> _) where T : UIView => nameof(Visible);

        public static BindablePropertyDescriptor<T, object?> Content<T>(this BindableMembersDescriptor<T> _) where T : UIView => nameof(Content);

        public static BindablePropertyDescriptor<T, Action<UIView, object?>?> ContentSetter<T>(this BindableMembersDescriptor<T> _) where T : UIView => nameof(ContentSetter);

        public static BindablePropertyDescriptor<T, IContentTemplateSelector?> ContentTemplateSelector<T>(this BindableMembersDescriptor<T> _) where T : UIView =>
            nameof(ContentTemplateSelector);

        public static BindablePropertyDescriptor<T, object?> ItemTemplateSelector<T>(this BindableMembersDescriptor<T> _) where T : UIView => nameof(ItemTemplateSelector);

        public static BindablePropertyDescriptor<T, object?> SelectedItem<T>(this BindableMembersDescriptor<T> _) where T : UIView => nameof(SelectedItem);

        public static BindableEventDescriptor<T> Click<T>(this BindableMembersDescriptor<T> _) where T : UIView => nameof(Click);

        public static BindableEventDescriptor<T> Refreshed<T>(this BindableMembersDescriptor<T> _) where T : UIRefreshControl => nameof(Refreshed);

        [BindingMember(nameof(ItemTemplateSelector))]
        public static object? ItemTemplateSelector<T>(this BindableMembersTargetDescriptor<T> descriptor) where T : UIView =>
            ItemTemplateSelector<T>(_: default).GetValue(descriptor.Target);

        public static void SetItemTemplateSelector<T>(this BindableMembersTargetDescriptor<T> descriptor, object? value) where T : UIView =>
            ItemTemplateSelector<T>(_: default).SetValue(descriptor.Target, value);

        [BindingMember(nameof(SelectedItem))]
        public static object? SelectedItem<T>(this BindableMembersTargetDescriptor<T> descriptor) where T : UIView => SelectedItem<T>(_: default).GetValue(descriptor.Target);

        public static void SetSelectedItem<T>(this BindableMembersTargetDescriptor<T> descriptor, object? value) where T : UIView =>
            SelectedItem<T>(_: default).SetValue(descriptor.Target, value);

        [BindingMember(nameof(Visible))]
        public static bool Visible<T>(this BindableMembersTargetDescriptor<T> descriptor) where T : UIView => Visible<T>(_: default).GetValue(descriptor.Target);

        public static void SetVisible<T>(this BindableMembersTargetDescriptor<T> descriptor, bool value) where T : UIView =>
            Visible<T>(_: default).SetValue(descriptor.Target, value);

        [BindingMember(nameof(Content))]
        public static object? Content<T>(this BindableMembersTargetDescriptor<T> descriptor) where T : UIView => Content<T>(_: default).GetValue(descriptor.Target);

        public static void SetContent<T>(this BindableMembersTargetDescriptor<T> descriptor, object? value) where T : UIView =>
            Content<T>(_: default).SetValue(descriptor.Target, value);

        [BindingMember(nameof(ContentSetter))]
        public static Action<UIView, object?>? ContentSetter<T>(this BindableMembersTargetDescriptor<T> descriptor) where T : UIView =>
            ContentSetter<T>(_: default).GetValue(descriptor.Target);

        public static void SetContent<T>(this BindableMembersTargetDescriptor<T> descriptor, Action<UIView, object?>? value) where T : UIView =>
            ContentSetter<T>(_: default).SetValue(descriptor.Target, value);

        [BindingMember(nameof(ContentTemplateSelector))]
        public static IContentTemplateSelector? ContentTemplateSelector<T>(this BindableMembersTargetDescriptor<T> descriptor) where T : UIView =>
            ContentTemplateSelector<T>(_: default).GetValue(descriptor.Target);

        public static void SetContentTemplateSelector<T>(this BindableMembersTargetDescriptor<T> descriptor, IContentTemplateSelector? value) where T : UIView =>
            ContentTemplateSelector<T>(_: default).SetValue(descriptor.Target, value);

        public static ActionToken AddClickListener<T>(this BindableMembersTargetDescriptor<T> descriptor, IEventListener listener) where T : UIView =>
            Click<T>(default).Subscribe(descriptor.Target, listener);

        public static ActionToken AddRefreshedListener<T>(this BindableMembersTargetDescriptor<T> descriptor, IEventListener listener) where T : UIRefreshControl =>
            Refreshed<T>(default).Subscribe(descriptor.Target, listener);
    }
}