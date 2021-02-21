using System.Runtime.CompilerServices;
using System.Windows;
using MugenMvvm.Bindings.Attributes;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Members.Descriptors;

namespace MugenMvvm.Windows.Bindings
{
    public static class WindowsBindableMembers
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BindablePropertyDescriptor<T, bool> Visible<T>(this BindableMembersDescriptor<T> _) where T : UIElement => nameof(Visible);

        [BindingMember(nameof(Visible))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Visible<T>(this BindableMembersTargetDescriptor<T> descriptor) where T : UIElement => Visible<T>(_: default).GetValue(descriptor.Target);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetVisible<T>(this BindableMembersTargetDescriptor<T> descriptor, bool value) where T : UIElement =>
            Visible<T>(_: default).SetValue(descriptor.Target, value);
    }
}