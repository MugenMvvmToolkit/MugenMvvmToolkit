using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MugenMvvm.Bindings.Attributes;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Members;
using MugenMvvm.Bindings.Members.Descriptors;
using MugenMvvm.Collections;
using MugenMvvm.Extensions;

namespace MugenMvvm.Bindings.Members
{
    public static class BindableMembers
    {
        private static MemberTypesRequest? _elementSourceMethod;
        private static MemberTypesRequest? _relativeSourceMethod;
        private static MemberTypesRequest? _hasErrorsMethod;
        private static MemberTypesRequest? _getErrorsMethod;
        private static MemberTypesRequest? _getErrorMethod;

        public static BindableMembersDescriptor<T> For<T>() where T : class => default;

        public static BindableMembersTargetDescriptor<T> For<T>(T target) where T : class => new(target);

        public static BindablePropertyDescriptor<T, object?> Root<T>(this BindableMembersDescriptor<T> _) where T : class => nameof(Root);

        public static BindablePropertyDescriptor<T, object?> Parent<T>(this BindableMembersDescriptor<T> _) where T : class => nameof(Parent);

        public static BindablePropertyDescriptor<T, object?> ParentNative<T>(this BindableMembersDescriptor<T> _) where T : class => nameof(ParentNative);

        public static BindablePropertyDescriptor<T, bool> Enabled<T>(this BindableMembersDescriptor<T> _) where T : class => nameof(Enabled);

        public static BindablePropertyDescriptor<T, object?> DataContext<T>(this BindableMembersDescriptor<T> _) where T : class => nameof(DataContext);

        public static BindablePropertyDescriptor<T, IEnumerable?> ItemsSourceRaw<T>(this BindableMembersDescriptor<T> _) where T : class => nameof(ItemsSourceRaw);

        public static BindablePropertyDescriptor<T, IEnumerable?> ItemsSource<T>(this BindableMembersDescriptor<T> _) where T : class => nameof(ItemsSource);

        public static BindableMethodDescriptor<T, object, object?> ElementSourceMethod<T>(this BindableMembersDescriptor<T> _) where T : class =>
            _elementSourceMethod ??= new MemberTypesRequest(nameof(ElementSource), typeof(object));

        public static BindableMethodDescriptor<T, string, int, object?> RelativeSourceMethod<T>(this BindableMembersDescriptor<T> _) where T : class =>
            _relativeSourceMethod ??= new MemberTypesRequest(nameof(RelativeSource), new[] {typeof(string), typeof(int)});

        public static BindableMethodDescriptor<T, object?, bool> HasErrorsMethod<T>(this BindableMembersDescriptor<T> _) where T : class =>
            _hasErrorsMethod ??= new MemberTypesRequest(nameof(HasErrors), typeof(object));

        public static BindableMethodDescriptor<T, object?, object?> GetErrorMethod<T>(this BindableMembersDescriptor<T> _) where T : class =>
            _getErrorMethod ??= new MemberTypesRequest(nameof(GetError), typeof(object));

        public static BindableMethodDescriptor<T, object?, IReadOnlyList<object>> GetErrorsMethod<T>(this BindableMembersDescriptor<T> _) where T : class =>
            _getErrorsMethod ??= new MemberTypesRequest(nameof(GetErrors), typeof(object));

        [BindingMember(nameof(Root))]
        public static object? Root<T>(this BindableMembersTargetDescriptor<T> descriptor) where T : class => Root<T>(_: default).GetValue(descriptor.Target);

        [BindingMember(nameof(Parent))]
        public static object? Parent<T>(this BindableMembersTargetDescriptor<T> descriptor) where T : class => Parent<T>(_: default).GetValue(descriptor.Target);

        public static void SetParent<T>(this BindableMembersTargetDescriptor<T> descriptor, object? value) where T : class =>
            Parent<T>(_: default).SetValue(descriptor.Target, value);

        [BindingMember(nameof(Enabled))]
        public static bool Enabled<T>(this BindableMembersTargetDescriptor<T> descriptor) where T : class => Enabled<T>(_: default).GetValue(descriptor.Target);

        public static void SetEnabled<T>(this BindableMembersTargetDescriptor<T> descriptor, bool value) where T : class =>
            Enabled<T>(_: default).SetValue(descriptor.Target, value);

        [BindingMember(nameof(DataContext))]
        public static object? DataContext<T>(this BindableMembersTargetDescriptor<T> descriptor) where T : class => DataContext<T>(_: default).GetValue(descriptor.Target);

        [BindingMember(nameof(DataContext))]
        public static T? DataContext<T>(this IBindableMembersTargetDescriptor<object> descriptor) where T : class =>
            ExceptionManager.ThrowBindableMemberNotSupported<T>(nameof(DataContext));

        public static void SetDataContext<T>(this BindableMembersTargetDescriptor<T> descriptor, object? value) where T : class =>
            DataContext<T>(_: default).SetValue(descriptor.Target, value);

        [BindingMember(nameof(ItemsSource))]
        public static IEnumerable? ItemsSource<T>(this BindableMembersTargetDescriptor<T> descriptor) where T : class =>
            ItemsSource<T>(_: default).GetValue(descriptor.Target);

        [BindingMember(nameof(ItemsSourceRaw))]
        public static IEnumerable? ItemsSourceRaw<T>(this BindableMembersTargetDescriptor<T> descriptor) where T : class =>
            ItemsSourceRaw<T>(_: default).GetValue(descriptor.Target);

        public static void SetItemsSource<T>(this BindableMembersTargetDescriptor<T> descriptor, IEnumerable? value) where T : class =>
            ItemsSourceRaw<T>(_: default).SetValue(descriptor.Target, value);

        public static object? ElementSource<T>(this BindableMembersTargetDescriptor<T> descriptor, object name) where T : class =>
            ElementSourceMethod<T>(default).Invoke(descriptor.Target, name);

        public static object? RelativeSource<T>(this BindableMembersTargetDescriptor<T> descriptor, string name, int level) where T : class =>
            RelativeSourceMethod<T>(default).Invoke(descriptor.Target, name, level);

        public static object? RelativeSource<T>(this BindableMembersTargetDescriptor<T> descriptor, string name) where T : class =>
            RelativeSourceMethod<T>(default).Invoke(descriptor.Target, name, 1);

        public static bool HasErrors<T>(this BindableMembersTargetDescriptor<T> descriptor, ItemOrArray<string> members) where T : class =>
            HasErrorsMethod<T>(default).Invoke(descriptor.Target, members.GetRawValue());

        public static object? GetError<T>(this BindableMembersTargetDescriptor<T> descriptor, ItemOrArray<string> members) where T : class =>
            GetErrorMethod<T>(default).Invoke(descriptor.Target, members.GetRawValue());

        public static IReadOnlyList<object> GetErrors<T>(this BindableMembersTargetDescriptor<T> descriptor, ItemOrArray<string> members) where T : class =>
            GetErrorsMethod<T>(default).Invoke(descriptor.Target, members.GetRawValue());
    }
}