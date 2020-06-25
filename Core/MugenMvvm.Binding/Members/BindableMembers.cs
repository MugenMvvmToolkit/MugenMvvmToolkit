using System.Collections.Generic;
using MugenMvvm.Binding.Attributes;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Members.Descriptors;

namespace MugenMvvm.Binding.Members
{
    public static class BindableMembers
    {
        #region Methods

        public static IBindableMembersDescriptor<T> For<T>() where T : class => null!;

        public static IBindableMembersDescriptor<T> For<T>(T? _) where T : class => null!;


        public static BindablePropertyDescriptor<object, object?> Root(this IBindableMembersDescriptor<object> _) => nameof(Root);

        public static BindablePropertyDescriptor<object, object?> Parent(this IBindableMembersDescriptor<object> _) => nameof(Parent);

        public static BindablePropertyDescriptor<object, bool> Enabled(this IBindableMembersDescriptor<object> _) => nameof(Enabled);

        public static BindablePropertyDescriptor<object, object?> DataContext(this IBindableMembersDescriptor<object> _) => nameof(DataContext);

        public static BindableMethodDescriptor<object, string, object?> ElementSourceMethod(this IBindableMembersDescriptor<object> _) => nameof(ElementSource);

        public static BindableMethodDescriptor<object, string, int, object?> RelativeSourceMethod(this IBindableMembersDescriptor<object> _) => nameof(RelativeSource);

        public static BindableMethodDescriptor<object, string[], bool> HasErrorsMethod(this IBindableMembersDescriptor<object> _) => "HasErrors";

        public static BindableMethodDescriptor<object, string[], IReadOnlyList<object>> GetErrorsMethod(this IBindableMembersDescriptor<object> _) => "GetErrors";

        public static BindableMethodDescriptor<object, string[], object?> GetErrorMethod(this IBindableMembersDescriptor<object> _) => "GetError";

        public static BindableMethodDescriptor<object, string, object?> FindElementByNameMethod(this IBindableMembersDescriptor<object> _) => "FindElementByName";


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