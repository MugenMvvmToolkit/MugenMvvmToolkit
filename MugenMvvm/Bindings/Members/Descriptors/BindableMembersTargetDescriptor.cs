using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MugenMvvm.Bindings.Members.Descriptors
{
    [StructLayout(LayoutKind.Auto)]
    public readonly ref struct BindableMembersTargetDescriptor<TTarget> where TTarget : class
    {
        public readonly TTarget Target;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BindableMembersTargetDescriptor(TTarget target)
        {
            Should.NotBeNull(target, nameof(target));
            Target = target;
        }

        public BindableMembersDescriptor<TTarget> Descriptor => default;
    }
}