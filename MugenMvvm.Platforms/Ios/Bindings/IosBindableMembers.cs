using System;
using System.Collections;
using System.Runtime.CompilerServices;
using Foundation;
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BindablePropertyDescriptor<T, bool> Visible<T>(this BindableMembersDescriptor<T> _) where T : UIView => nameof(Visible);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BindablePropertyDescriptor<T, object?> Content<T>(this BindableMembersDescriptor<T> _) where T : UIView => nameof(Content);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BindablePropertyDescriptor<T, Action<UIView, object?>?> ContentSetter<T>(this BindableMembersDescriptor<T> _) where T : UIView => nameof(ContentSetter);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BindablePropertyDescriptor<T, IContentTemplateSelector?> ContentTemplateSelector<T>(this BindableMembersDescriptor<T> _) where T : UIView =>
            nameof(ContentTemplateSelector);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BindablePropertyDescriptor<T, ICollectionViewManager?> CollectionViewManager<T>(this BindableMembersDescriptor<T> _) where T : NSObject =>
            nameof(CollectionViewManager);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BindablePropertyDescriptor<T, IEnumerable?> ItemsSource<T>(this BindableMembersDescriptor<T> _) where T : UIView => nameof(ItemsSource);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BindablePropertyDescriptor<T, object?> ItemTemplateSelector<T>(this BindableMembersDescriptor<T> _) where T : UIView => nameof(ItemTemplateSelector);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BindablePropertyDescriptor<T, object?> SelectedItem<T>(this BindableMembersDescriptor<T> _) where T : UIView => nameof(SelectedItem);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BindableEventDescriptor<T> Click<T>(this BindableMembersDescriptor<T> _) where T : UIView => nameof(Click);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BindableEventDescriptor<T> Refreshed<T>(this BindableMembersDescriptor<T> _) where T : UIRefreshControl => nameof(Refreshed);

        [BindingMember(nameof(CollectionViewManager))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ICollectionViewManager? CollectionViewManager<T>(this BindableMembersTargetDescriptor<T> descriptor) where T : NSObject =>
            CollectionViewManager<T>(_: default).GetValue(descriptor.Target);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetCollectionViewManager<T>(this BindableMembersTargetDescriptor<T> descriptor, ICollectionViewManager? value) where T : NSObject =>
            CollectionViewManager<T>(_: default).SetValue(descriptor.Target, value);

        [BindingMember(nameof(ItemTemplateSelector))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object? ItemTemplateSelector<T>(this BindableMembersTargetDescriptor<T> descriptor) where T : UIView =>
            ItemTemplateSelector<T>(_: default).GetValue(descriptor.Target);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetItemTemplateSelector<T>(this BindableMembersTargetDescriptor<T> descriptor, object? value) where T : UIView =>
            ItemTemplateSelector<T>(_: default).SetValue(descriptor.Target, value);

        [BindingMember(nameof(ItemsSource))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable? ItemsSource<T>(this BindableMembersTargetDescriptor<T> descriptor) where T : UIView =>
            ItemsSource<T>(_: default).GetValue(descriptor.Target);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetItemsSource<T>(this BindableMembersTargetDescriptor<T> descriptor, IEnumerable? value) where T : UIView =>
            ItemsSource<T>(_: default).SetValue(descriptor.Target, value);

        [BindingMember(nameof(SelectedItem))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object? SelectedItem<T>(this BindableMembersTargetDescriptor<T> descriptor) where T : UIView => SelectedItem<T>(_: default).GetValue(descriptor.Target);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetSelectedItem<T>(this BindableMembersTargetDescriptor<T> descriptor, object? value) where T : UIView =>
            SelectedItem<T>(_: default).SetValue(descriptor.Target, value);

        [BindingMember(nameof(Visible))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Visible<T>(this BindableMembersTargetDescriptor<T> descriptor) where T : UIView => Visible<T>(_: default).GetValue(descriptor.Target);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetVisible<T>(this BindableMembersTargetDescriptor<T> descriptor, bool value) where T : UIView =>
            Visible<T>(_: default).SetValue(descriptor.Target, value);

        [BindingMember(nameof(Content))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object? Content<T>(this BindableMembersTargetDescriptor<T> descriptor) where T : UIView => Content<T>(_: default).GetValue(descriptor.Target);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetContent<T>(this BindableMembersTargetDescriptor<T> descriptor, object? value) where T : UIView =>
            Content<T>(_: default).SetValue(descriptor.Target, value);

        [BindingMember(nameof(ContentSetter))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Action<UIView, object?>? ContentSetter<T>(this BindableMembersTargetDescriptor<T> descriptor) where T : UIView =>
            ContentSetter<T>(_: default).GetValue(descriptor.Target);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetContent<T>(this BindableMembersTargetDescriptor<T> descriptor, Action<UIView, object?>? value) where T : UIView =>
            ContentSetter<T>(_: default).SetValue(descriptor.Target, value);

        [BindingMember(nameof(ContentTemplateSelector))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IContentTemplateSelector? ContentTemplateSelector<T>(this BindableMembersTargetDescriptor<T> descriptor) where T : UIView =>
            ContentTemplateSelector<T>(_: default).GetValue(descriptor.Target);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetContentTemplateSelector<T>(this BindableMembersTargetDescriptor<T> descriptor, IContentTemplateSelector? value) where T : UIView =>
            ContentTemplateSelector<T>(_: default).SetValue(descriptor.Target, value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ActionToken AddClickListener<T>(this BindableMembersTargetDescriptor<T> descriptor, IEventListener listener) where T : UIView =>
            Click<T>(default).Subscribe(descriptor.Target, listener);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ActionToken AddRefreshedListener<T>(this BindableMembersTargetDescriptor<T> descriptor, IEventListener listener) where T : UIRefreshControl =>
            Refreshed<T>(default).Subscribe(descriptor.Target, listener);
    }
}