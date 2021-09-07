using System.Collections;
using Android.Views;
using MugenMvvm.Android.Interfaces;
using MugenMvvm.Bindings.Attributes;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Bindings.Members.Descriptors;
using MugenMvvm.Internal;

namespace MugenMvvm.Android.Bindings
{
    public static class MenuBindableMembers
    {
        public static BindablePropertyDescriptor<T, IEnumerable?> ItemsSource<T>(this BindableMembersDescriptor<T> _) where T : class, IMenu => nameof(ItemsSource);

        public static BindablePropertyDescriptor<T, IMenuItemTemplate?> ItemTemplate<T>(this BindableMembersDescriptor<T> _) where T : class, IMenu => nameof(ItemTemplate);

        public static BindablePropertyDescriptor<T, string?> Title<T>(this BindableMembersDescriptor<T> _) where T : class, IMenuItem => nameof(Title);

        public static BindableEventDescriptor<T> Click<T>(this BindableMembersDescriptor<T> _) where T : class, IMenuItem => nameof(Click);

        [BindingMember(nameof(ItemsSource))]
        public static IEnumerable? ItemsSource<T>(this BindableMembersTargetDescriptor<T> descriptor) where T : class, IMenu =>
            ItemsSource<T>(_: default).GetValue(descriptor.Target);

        public static void SetItemsSource<T>(this BindableMembersTargetDescriptor<T> descriptor, IEnumerable? value) where T : class, IMenu =>
            ItemsSource<T>(_: default).SetValue(descriptor.Target, value);

        [BindingMember(nameof(ItemTemplate))]
        public static IMenuItemTemplate? ItemTemplate<T>(this BindableMembersTargetDescriptor<T> descriptor) where T : class, IMenu =>
            ItemTemplate<T>(_: default).GetValue(descriptor.Target);

        public static void SetItemTemplate<T>(this BindableMembersTargetDescriptor<T> descriptor, IMenuItemTemplate? value) where T : class, IMenu =>
            ItemTemplate<T>(_: default).SetValue(descriptor.Target, value);

        [BindingMember(nameof(Title))]
        public static string? Title<T>(this BindableMembersTargetDescriptor<T> descriptor) where T : class, IMenuItem =>
            Title<T>(_: default).GetValue(descriptor.Target);

        public static void SetTitle<T>(this BindableMembersTargetDescriptor<T> descriptor, string? value) where T : class, IMenuItem =>
            Title<T>(_: default).SetValue(descriptor.Target, value);

        public static ActionToken AddClickListener<T>(this BindableMembersTargetDescriptor<T> descriptor, IEventListener listener) where T : class, IMenuItem =>
            Click<T>(default).Subscribe(descriptor.Target, listener);
    }
}