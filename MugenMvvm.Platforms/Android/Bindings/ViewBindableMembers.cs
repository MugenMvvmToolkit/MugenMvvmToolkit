using System.Runtime.CompilerServices;
using Android.Views;
using Java.Lang;
using MugenMvvm.Android.Interfaces;
using MugenMvvm.Bindings.Attributes;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Bindings.Members.Descriptors;
using MugenMvvm.Internal;

namespace MugenMvvm.Android.Bindings
{
    public static class ViewBindableMembers
    {
        public static BindablePropertyDescriptor<T, IStableIdProvider?> StableIdProvider<T>(this BindableMembersDescriptor<T> _) where T : View => nameof(StableIdProvider);

        public static BindablePropertyDescriptor<T, object?> ItemTemplateSelector<T>(this BindableMembersDescriptor<T> _) where T : View => nameof(ItemTemplateSelector);

        public static BindablePropertyDescriptor<T, IMenuTemplate?> MenuTemplate<T>(this BindableMembersDescriptor<T> _) where T : View => nameof(MenuTemplate);

        public static BindablePropertyDescriptor<T, bool> Visible<T>(this BindableMembersDescriptor<T> _) where T : View => nameof(Visible);

        public static BindablePropertyDescriptor<T, bool> Invisible<T>(this BindableMembersDescriptor<T> _) where T : View => nameof(Invisible);

        public static BindablePropertyDescriptor<T, object?> Content<T>(this BindableMembersDescriptor<T> _) where T : View => nameof(Content);

        public static BindablePropertyDescriptor<T, object?> ContentTemplateSelector<T>(this BindableMembersDescriptor<T> _) where T : View => nameof(ContentTemplateSelector);

        public static BindablePropertyDescriptor<T, int> SelectedIndex<T>(this BindableMembersDescriptor<T> _) where T : View => nameof(SelectedIndex);
        
        public static BindablePropertyDescriptor<T, object?> SelectedItem<T>(this BindableMembersDescriptor<T> _) where T : View => nameof(SelectedItem);
        
        public static BindablePropertyDescriptor<T, bool> SmoothScroll<T>(this BindableMembersDescriptor<T> _) where T : View => nameof(SmoothScroll);
        
        public static BindablePropertyDescriptor<T, bool> Refreshing<T>(this BindableMembersDescriptor<T> _) where T : View => nameof(Refreshing);
        
        public static BindableEventDescriptor<T> Click<T>(this BindableMembersDescriptor<T> _) where T : View => nameof(Click);
        
        public static BindableEventDescriptor<T> ParentChanged<T>(this BindableMembersDescriptor<T> _) where T : View => nameof(ParentChanged);
        
        public static BindableEventDescriptor<T> TextChanged<T>(this BindableMembersDescriptor<T> _) where T : View => nameof(TextChanged);
        
        public static BindableEventDescriptor<T> Refreshed<T>(this BindableMembersDescriptor<T> _) where T : View => nameof(Refreshed);
        
        public static BindableEventDescriptor<T> SelectedIndexChanged<T>(this BindableMembersDescriptor<T> _) where T : View => nameof(SelectedIndexChanged);
        
        public static BindableEventDescriptor<T> SelectedItemChanged<T>(this BindableMembersDescriptor<T> _) where T : View => nameof(SelectedItemChanged);
        
        public static BindableEventDescriptor<T> ActionBarHomeButtonClick<T>(this BindableMembersDescriptor<T> _) where T : Object => ViewMemberChangedListener.HomeButtonClick;

        [BindingMember(nameof(StableIdProvider))]
        public static IStableIdProvider? StableIdProvider<T>(this BindableMembersTargetDescriptor<T> descriptor) where T : View =>
            StableIdProvider<T>(_: default).GetValue(descriptor.Target);
        
        public static void SetStableIdProvider<T>(this BindableMembersTargetDescriptor<T> descriptor, IStableIdProvider? value) where T : View =>
            StableIdProvider<T>(_: default).SetValue(descriptor.Target, value);

        [BindingMember(nameof(ItemTemplateSelector))]
        public static object? ItemTemplateSelector<T>(this BindableMembersTargetDescriptor<T> descriptor) where T : View =>
            ItemTemplateSelector<T>(_: default).GetValue(descriptor.Target);
        
        public static void SetItemTemplateSelector<T>(this BindableMembersTargetDescriptor<T> descriptor, object? value) where T : View =>
            ItemTemplateSelector<T>(_: default).SetValue(descriptor.Target, value);

        [BindingMember(nameof(MenuTemplate))]
        public static IMenuTemplate? MenuTemplate<T>(this BindableMembersTargetDescriptor<T> descriptor) where T : View => MenuTemplate<T>(_: default).GetValue(descriptor.Target);
        
        public static void SetMenuTemplate<T>(this BindableMembersTargetDescriptor<T> descriptor, IMenuTemplate? value) where T : View =>
            MenuTemplate<T>(_: default).SetValue(descriptor.Target, value);

        [BindingMember(nameof(Visible))]
        public static bool Visible<T>(this BindableMembersTargetDescriptor<T> descriptor) where T : View => Visible<T>(_: default).GetValue(descriptor.Target);
        
        public static void SetVisible<T>(this BindableMembersTargetDescriptor<T> descriptor, bool value) where T : View =>
            Visible<T>(_: default).SetValue(descriptor.Target, value);

        [BindingMember(nameof(Invisible))]
        public static bool Invisible<T>(this BindableMembersTargetDescriptor<T> descriptor) where T : View => Invisible<T>(_: default).GetValue(descriptor.Target);
        
        public static void SetInvisible<T>(this BindableMembersTargetDescriptor<T> descriptor, bool value) where T : View =>
            Invisible<T>(_: default).SetValue(descriptor.Target, value);

        [BindingMember(nameof(Content))]
        public static object? Content<T>(this BindableMembersTargetDescriptor<T> descriptor) where T : View => Content<T>(_: default).GetValue(descriptor.Target);
        
        public static void SetContent<T>(this BindableMembersTargetDescriptor<T> descriptor, object? value) where T : View =>
            Content<T>(_: default).SetValue(descriptor.Target, value);

        [BindingMember(nameof(ContentTemplateSelector))]
        public static IContentTemplateSelector? ContentTemplateSelector<T>(this BindableMembersTargetDescriptor<T> descriptor) where T : View =>
            (IContentTemplateSelector?) ContentTemplateSelector<T>(_: default).GetValue(descriptor.Target);
        
        public static void SetContentTemplateSelector<T>(this BindableMembersTargetDescriptor<T> descriptor, IContentTemplateSelector? value) where T : View =>
            ContentTemplateSelector<T>(_: default).SetValue(descriptor.Target, value);

        [BindingMember(nameof(SelectedIndex))]
        public static int SelectedIndex<T>(this BindableMembersTargetDescriptor<T> descriptor) where T : View => SelectedIndex<T>(_: default).GetValue(descriptor.Target);
        
        public static void SetSelectedIndex<T>(this BindableMembersTargetDescriptor<T> descriptor, int value) where T : View =>
            SelectedIndex<T>(_: default).SetValue(descriptor.Target, value);

        [BindingMember(nameof(SelectedItem))]
        public static object? SelectedItem<T>(this BindableMembersTargetDescriptor<T> descriptor) where T : View => SelectedItem<T>(_: default).GetValue(descriptor.Target);
        
        public static void SetSelectedItem<T>(this BindableMembersTargetDescriptor<T> descriptor, object? value) where T : View =>
            SelectedItem<T>(_: default).SetValue(descriptor.Target, value);

        [BindingMember(nameof(SmoothScroll))]
        public static bool SmoothScroll<T>(this BindableMembersTargetDescriptor<T> descriptor) where T : View => SmoothScroll<T>(_: default).GetValue(descriptor.Target);
        
        public static void SetSmoothScroll<T>(this BindableMembersTargetDescriptor<T> descriptor, bool value) where T : View =>
            SmoothScroll<T>(_: default).SetValue(descriptor.Target, value);

        [BindingMember(nameof(Refreshing))]
        public static bool Refreshing<T>(this BindableMembersTargetDescriptor<T> descriptor) where T : View => Refreshing<T>(_: default).GetValue(descriptor.Target);
        
        public static void SetRefreshing<T>(this BindableMembersTargetDescriptor<T> descriptor, bool value) where T : View =>
            Refreshing<T>(_: default).SetValue(descriptor.Target, value);
        
        public static ActionToken AddClickListener<T>(this BindableMembersTargetDescriptor<T> descriptor, IEventListener listener) where T : View =>
            Click<T>(default).Subscribe(descriptor.Target, listener);
        
        public static ActionToken AddParentChangedListener<T>(this BindableMembersTargetDescriptor<T> descriptor, IEventListener listener) where T : View =>
            ParentChanged<T>(default).Subscribe(descriptor.Target, listener);
        
        public static ActionToken AddTextChangedListener<T>(this BindableMembersTargetDescriptor<T> descriptor, IEventListener listener) where T : View =>
            TextChanged<T>(default).Subscribe(descriptor.Target, listener);
        
        public static ActionToken AddSelectedIndexChangedListener<T>(this BindableMembersTargetDescriptor<T> descriptor, IEventListener listener) where T : View =>
            SelectedIndexChanged<T>(default).Subscribe(descriptor.Target, listener);
        
        public static ActionToken AddSelectedItemChangedListener<T>(this BindableMembersTargetDescriptor<T> descriptor, IEventListener listener) where T : View =>
            SelectedItemChanged<T>(default).Subscribe(descriptor.Target, listener);
        
        public static ActionToken AddActionBarHomeButtonClick<T>(this BindableMembersTargetDescriptor<T> descriptor, IEventListener listener) where T : Object =>
            ActionBarHomeButtonClick<T>(default).Subscribe(descriptor.Target, listener);
    }
}