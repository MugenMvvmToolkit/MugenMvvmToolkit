using System.Collections;
using System.Runtime.CompilerServices;
using Android.Views;
using Java.Lang;
using MugenMvvm.Android.Interfaces;
using MugenMvvm.Android.Observation;
using MugenMvvm.Bindings.Attributes;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Bindings.Members.Descriptors;
using MugenMvvm.Internal;

namespace MugenMvvm.Android.Members
{
    public static class ViewBindableMembers
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BindablePropertyDescriptor<T, ICollectionViewManager?> CollectionViewManager<T>(this BindableMembersDescriptor<T> _) where T : Object =>
            nameof(CollectionViewManager);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BindablePropertyDescriptor<T, IStableIdProvider?> StableIdProvider<T>(this BindableMembersDescriptor<T> _) where T : View => nameof(StableIdProvider);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BindablePropertyDescriptor<T, object?> ItemTemplateSelector<T>(this BindableMembersDescriptor<T> _) where T : View => nameof(ItemTemplateSelector);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BindablePropertyDescriptor<T, IEnumerable?> ItemsSource<T>(this BindableMembersDescriptor<T> _) where T : View => nameof(ItemsSource);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BindablePropertyDescriptor<T, IMenuTemplate?> MenuTemplate<T>(this BindableMembersDescriptor<T> _) where T : View => nameof(MenuTemplate);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BindablePropertyDescriptor<T, bool> Visible<T>(this BindableMembersDescriptor<T> _) where T : View => nameof(Visible);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BindablePropertyDescriptor<T, bool> Invisible<T>(this BindableMembersDescriptor<T> _) where T : View => nameof(Invisible);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BindablePropertyDescriptor<T, object?> Content<T>(this BindableMembersDescriptor<T> _) where T : View => nameof(Content);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BindablePropertyDescriptor<T, object?> ContentTemplateSelector<T>(this BindableMembersDescriptor<T> _) where T : View => nameof(ContentTemplateSelector);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BindablePropertyDescriptor<T, int> SelectedIndex<T>(this BindableMembersDescriptor<T> _) where T : View => nameof(SelectedIndex);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BindablePropertyDescriptor<T, object?> SelectedItem<T>(this BindableMembersDescriptor<T> _) where T : View => nameof(SelectedItem);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BindablePropertyDescriptor<T, bool> SmoothScroll<T>(this BindableMembersDescriptor<T> _) where T : View => nameof(SmoothScroll);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BindableEventDescriptor<T> Click<T>(this BindableMembersDescriptor<T> _) where T : View => nameof(Click);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BindableEventDescriptor<T> ParentChanged<T>(this BindableMembersDescriptor<T> _) where T : View => nameof(ParentChanged);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BindableEventDescriptor<T> TextChanged<T>(this BindableMembersDescriptor<T> _) where T : View => nameof(TextChanged);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BindableEventDescriptor<T> Refreshed<T>(this BindableMembersDescriptor<T> _) where T : View => nameof(Refreshed);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BindableEventDescriptor<T> SelectedIndexChanged<T>(this BindableMembersDescriptor<T> _) where T : View => nameof(SelectedIndexChanged);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BindableEventDescriptor<T> SelectedItemChanged<T>(this BindableMembersDescriptor<T> _) where T : View => nameof(SelectedItemChanged);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BindableEventDescriptor<T> ActionBarHomeButtonClick<T>(this BindableMembersDescriptor<T> _) where T : Object => ViewMemberChangedListener.HomeButtonClick;

        [BindingMember(nameof(CollectionViewManager))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ICollectionViewManager? CollectionViewManager<T>(this BindableMembersTargetDescriptor<T> descriptor) where T : Object =>
            CollectionViewManager<T>(_: default).GetValue(descriptor.Target);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetCollectionViewManager<T>(this BindableMembersTargetDescriptor<T> descriptor, ICollectionViewManager? value) where T : Object =>
            CollectionViewManager<T>(_: default).SetValue(descriptor.Target, value);

        [BindingMember(nameof(StableIdProvider))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IStableIdProvider? StableIdProvider<T>(this BindableMembersTargetDescriptor<T> descriptor) where T : View =>
            StableIdProvider<T>(_: default).GetValue(descriptor.Target);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetStableIdProvider<T>(this BindableMembersTargetDescriptor<T> descriptor, IStableIdProvider? value) where T : View =>
            StableIdProvider<T>(_: default).SetValue(descriptor.Target, value);

        [BindingMember(nameof(ItemTemplateSelector))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object? ItemTemplateSelector<T>(this BindableMembersTargetDescriptor<T> descriptor) where T : View =>
            ItemTemplateSelector<T>(_: default).GetValue(descriptor.Target);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetItemTemplateSelector<T>(this BindableMembersTargetDescriptor<T> descriptor, object? value) where T : View =>
            ItemTemplateSelector<T>(_: default).SetValue(descriptor.Target, value);

        [BindingMember(nameof(ItemsSource))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable? ItemsSource<T>(this BindableMembersTargetDescriptor<T> descriptor) where T : View =>
            ItemsSource<T>(_: default).GetValue(descriptor.Target);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetItemsSource<T>(this BindableMembersTargetDescriptor<T> descriptor, IEnumerable? value) where T : View =>
            ItemsSource<T>(_: default).SetValue(descriptor.Target, value);

        [BindingMember(nameof(MenuTemplate))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IMenuTemplate? MenuTemplate<T>(this BindableMembersTargetDescriptor<T> descriptor) where T : View => MenuTemplate<T>(_: default).GetValue(descriptor.Target);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetMenuTemplate<T>(this BindableMembersTargetDescriptor<T> descriptor, IMenuTemplate? value) where T : View =>
            MenuTemplate<T>(_: default).SetValue(descriptor.Target, value);

        [BindingMember(nameof(Visible))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Visible<T>(this BindableMembersTargetDescriptor<T> descriptor) where T : View => Visible<T>(_: default).GetValue(descriptor.Target);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetVisible<T>(this BindableMembersTargetDescriptor<T> descriptor, bool value) where T : View =>
            Visible<T>(_: default).SetValue(descriptor.Target, value);

        [BindingMember(nameof(Invisible))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Invisible<T>(this BindableMembersTargetDescriptor<T> descriptor) where T : View => Invisible<T>(_: default).GetValue(descriptor.Target);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetInvisible<T>(this BindableMembersTargetDescriptor<T> descriptor, bool value) where T : View =>
            Invisible<T>(_: default).SetValue(descriptor.Target, value);

        [BindingMember(nameof(Content))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object? Content<T>(this BindableMembersTargetDescriptor<T> descriptor) where T : View => Content<T>(_: default).GetValue(descriptor.Target);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetContent<T>(this BindableMembersTargetDescriptor<T> descriptor, object? value) where T : View =>
            Content<T>(_: default).SetValue(descriptor.Target, value);

        [BindingMember(nameof(ContentTemplateSelector))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IContentTemplateSelector? ContentTemplateSelector<T>(this BindableMembersTargetDescriptor<T> descriptor) where T : View =>
            (IContentTemplateSelector?) ContentTemplateSelector<T>(_: default).GetValue(descriptor.Target);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetContentTemplateSelector<T>(this BindableMembersTargetDescriptor<T> descriptor, IContentTemplateSelector? value) where T : View =>
            ContentTemplateSelector<T>(_: default).SetValue(descriptor.Target, value);

        [BindingMember(nameof(SelectedIndex))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int SelectedIndex<T>(this BindableMembersTargetDescriptor<T> descriptor) where T : View => SelectedIndex<T>(_: default).GetValue(descriptor.Target);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetSelectedIndex<T>(this BindableMembersTargetDescriptor<T> descriptor, int value) where T : View =>
            SelectedIndex<T>(_: default).SetValue(descriptor.Target, value);

        [BindingMember(nameof(SelectedItem))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object? SelectedItem<T>(this BindableMembersTargetDescriptor<T> descriptor) where T : View => SelectedItem<T>(_: default).GetValue(descriptor.Target);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetSelectedItem<T>(this BindableMembersTargetDescriptor<T> descriptor, object? value) where T : View =>
            SelectedItem<T>(_: default).SetValue(descriptor.Target, value);

        [BindingMember(nameof(SmoothScroll))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool SmoothScroll<T>(this BindableMembersTargetDescriptor<T> descriptor) where T : View => SmoothScroll<T>(_: default).GetValue(descriptor.Target);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetSmoothScroll<T>(this BindableMembersTargetDescriptor<T> descriptor, bool value) where T : View =>
            SmoothScroll<T>(_: default).SetValue(descriptor.Target, value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ActionToken AddClickListener<T>(this BindableMembersTargetDescriptor<T> descriptor, IEventListener listener) where T : View =>
            Click<T>(default).Subscribe(descriptor.Target, listener);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ActionToken AddParentChangedListener<T>(this BindableMembersTargetDescriptor<T> descriptor, IEventListener listener) where T : View =>
            ParentChanged<T>(default).Subscribe(descriptor.Target, listener);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ActionToken AddTextChangedListener<T>(this BindableMembersTargetDescriptor<T> descriptor, IEventListener listener) where T : View =>
            TextChanged<T>(default).Subscribe(descriptor.Target, listener);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ActionToken AddSelectedIndexChangedListener<T>(this BindableMembersTargetDescriptor<T> descriptor, IEventListener listener) where T : View =>
            SelectedIndexChanged<T>(default).Subscribe(descriptor.Target, listener);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ActionToken AddSelectedItemChangedListener<T>(this BindableMembersTargetDescriptor<T> descriptor, IEventListener listener) where T : View =>
            SelectedItemChanged<T>(default).Subscribe(descriptor.Target, listener);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ActionToken AddActionBarHomeButtonClick<T>(this BindableMembersTargetDescriptor<T> descriptor, IEventListener listener) where T : Object =>
            ActionBarHomeButtonClick<T>(default).Subscribe(descriptor.Target, listener);
    }
}