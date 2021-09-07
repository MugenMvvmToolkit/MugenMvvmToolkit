using Avalonia.Controls;
using MugenMvvm.Bindings.Attributes;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Members.Descriptors;
using MugenMvvm.Interfaces.Collections;

namespace MugenMvvm.Avalonia.Bindings
{
    public static class AvaloniaBindableMembers
    {
        public static BindablePropertyDescriptor<T, IDiffableEqualityComparer?> DiffableEqualityComparer<T>(this BindableMembersDescriptor<T> _) where T : ItemsControl =>
            nameof(DiffableEqualityComparer);

        [BindingMember(nameof(DiffableEqualityComparer))]
        public static IDiffableEqualityComparer? DiffableEqualityComparer<T>(this BindableMembersTargetDescriptor<T> descriptor) where T : ItemsControl =>
            DiffableEqualityComparer<T>(_: default).GetValue(descriptor.Target);

        public static void SetDiffableEqualityComparer<T>(this BindableMembersTargetDescriptor<T> descriptor, IDiffableEqualityComparer? value) where T : ItemsControl =>
            DiffableEqualityComparer<T>(_: default).SetValue(descriptor.Target, value);
    }
}