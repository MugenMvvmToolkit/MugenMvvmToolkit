using System.Collections.Generic;
using MugenMvvm.Binding.Attributes;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Members.Descriptors;

namespace MugenMvvm.Binding.Members
{
    public static class BindableMembers
    {
        #region Methods

        public static IBindableMembersDescriptor<T> For<T>() where T : class => null!;

        public static IBindableMembersDescriptor<T> For<T>(T? _) where T : class => null!;


        public static BindablePropertyDescriptor<T, object?> Root<T>(this IBindableMembersDescriptor<T> _) where T : class => nameof(Root);

        public static BindablePropertyDescriptor<T, object?> Parent<T>(this IBindableMembersDescriptor<T> _) where T : class => nameof(Parent);

        public static BindablePropertyDescriptor<T, object?> ParentNative<T>(this IBindableMembersDescriptor<T> _) where T : class => nameof(ParentNative);

        public static BindablePropertyDescriptor<T, bool> Enabled<T>(this IBindableMembersDescriptor<T> _) where T : class => nameof(Enabled);

        public static BindablePropertyDescriptor<T, object?> DataContext<T>(this IBindableMembersDescriptor<T> _) where T : class => nameof(DataContext);

        public static BindablePropertyDescriptor<T, object?> CommandParameter<T>(this IBindableMembersDescriptor<T> _) where T : class => nameof(CommandParameter);

        public static BindableMethodDescriptor<T, string, object?> ElementSourceMethod<T>(this IBindableMembersDescriptor<T> _) where T : class => nameof(ElementSource);

        public static BindableMethodDescriptor<T, string, int, object?> RelativeSourceMethod<T>(this IBindableMembersDescriptor<T> _) where T : class => nameof(RelativeSource);

        public static BindableMethodDescriptor<T, string[], bool> HasErrorsMethod<T>(this IBindableMembersDescriptor<T> _) where T : class => BindingInternalConstant.HasErrorsName;

        public static BindableMethodDescriptor<T, string[], IReadOnlyList<object>> GetErrorsMethod<T>(this IBindableMembersDescriptor<T> _) where T : class => BindingInternalConstant.GetErrorsName;

        public static BindableMethodDescriptor<T, string[], object?> GetErrorMethod<T>(this IBindableMembersDescriptor<T> _) where T : class => BindingInternalConstant.GetErrorName;


        [BindingMember(nameof(Root))]
        public static object? Root(this IBindableMembersBuildingDescriptor<object> _) => default!;

        [BindingMember(nameof(Parent))]
        public static object? Parent(this IBindableMembersBuildingDescriptor<object> _) => default!;

        [BindingMember(nameof(Enabled))]
        public static bool Enabled(this IBindableMembersBuildingDescriptor<object> _) => default!;

        [BindingMember(nameof(DataContext))]
        public static object DataContext(this IBindableMembersBuildingDescriptor<object> _) => default!;

        [BindingMember(nameof(DataContext))]
        public static T DataContext<T>(this IBindableMembersBuildingDescriptor<object> _) => default!;

        public static T ElementSource<T>(this IBindableMembersBuildingDescriptor<object> _, string name) => default!;

        public static T RelativeSource<T>(this IBindableMembersBuildingDescriptor<object> _, string typeName) => default!;

        public static T RelativeSource<T>(this IBindableMembersBuildingDescriptor<object> _, string typeName, int level) => default!;

        public static bool HasErrors(this IBindableMembersBuildingDescriptor<object> _, params object?[] members) => default;

        public static object? GetError(this IBindableMembersBuildingDescriptor<object> _, params object?[] members) => default;

        public static IReadOnlyList<object> GetErrors(this IBindableMembersBuildingDescriptor<object> _, params object?[] members) => default!;

        #endregion
    }
}