using System.Collections;
using MugenMvvm.Android.Interfaces;
using MugenMvvm.Android.Native.Interfaces.Views;
using MugenMvvm.Binding.Attributes;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Interfaces.Observation;
using MugenMvvm.Binding.Members.Descriptors;
using MugenMvvm.Internal;

namespace MugenMvvm.Android.Members
{
    public static class AndroidNativeBindableMembers
    {
        #region Methods

        public static BindablePropertyDescriptor<T, IStableIdProvider?> StableIdProvider<T>(this BindableMembersDescriptor<T> _) where T : class, IListView => nameof(StableIdProvider);

        public static BindablePropertyDescriptor<T, IDataTemplateSelector?> ItemTemplateSelector<T>(this BindableMembersDescriptor<T> _) where T : class, IListView => nameof(ItemTemplateSelector);

        public static BindablePropertyDescriptor<T, IEnumerable?> ItemsSource<T>(this BindableMembersDescriptor<T> _) where T : class, IListView => nameof(ItemsSource);

        public static BindableEventDescriptor<T> Click<T>(this BindableMembersDescriptor<T> _) where T : class, IAndroidView => nameof(Click);

        public static BindableEventDescriptor<T> ParentChanged<T>(this BindableMembersDescriptor<T> _) where T : class, IAndroidView => nameof(ParentChanged);

        public static BindableEventDescriptor<T> TextChanged<T>(this BindableMembersDescriptor<T> _) where T : class, ITextView => nameof(TextChanged);


        [BindingMember(nameof(StableIdProvider))]
        public static IStableIdProvider? StableIdProvider<T>(this BindableMembersTargetDescriptor<T> descriptor) where T : class, IListView => StableIdProvider<T>(_: default).GetValue(descriptor.Target);

        public static void SetStableIdProvider<T>(this BindableMembersTargetDescriptor<T> descriptor, IStableIdProvider? value) where T : class, IListView =>
            StableIdProvider<T>(_: default).SetValue(descriptor.Target, value);

        [BindingMember(nameof(ItemTemplateSelector))]
        public static IDataTemplateSelector? ItemTemplateSelector<T>(this BindableMembersTargetDescriptor<T> descriptor) where T : class, IListView =>
            ItemTemplateSelector<T>(_: default).GetValue(descriptor.Target);

        public static void SetItemTemplateSelector<T>(this BindableMembersTargetDescriptor<T> descriptor, IDataTemplateSelector? value) where T : class, IListView =>
            ItemTemplateSelector<T>(_: default).SetValue(descriptor.Target, value);

        [BindingMember(nameof(ItemsSource))]
        public static IEnumerable? ItemsSource<T>(this BindableMembersTargetDescriptor<T> descriptor) where T : class, IListView =>
            ItemsSource<T>(_: default).GetValue(descriptor.Target);

        public static void SetItemsSource<T>(this BindableMembersTargetDescriptor<T> descriptor, IEnumerable? value) where T : class, IListView =>
            ItemsSource<T>(_: default).SetValue(descriptor.Target, value);

        public static ActionToken AddClickListener<T>(this BindableMembersTargetDescriptor<T> descriptor, IEventListener listener) where T : class, IAndroidView =>
            Click<T>(default).Subscribe(descriptor.Target, listener);

        public static ActionToken AddParentChangedListener<T>(this BindableMembersTargetDescriptor<T> descriptor, IEventListener listener) where T : class, IAndroidView =>
            ParentChanged<T>(default).Subscribe(descriptor.Target, listener);

        public static ActionToken AddTextChangedListener<T>(this BindableMembersTargetDescriptor<T> descriptor, IEventListener listener) where T : class, ITextView =>
            TextChanged<T>(default).Subscribe(descriptor.Target, listener);

        #endregion
    }
}