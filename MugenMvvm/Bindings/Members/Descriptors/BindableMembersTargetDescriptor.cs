using System.Runtime.InteropServices;
using MugenMvvm.Bindings.Interfaces.Members;

namespace MugenMvvm.Bindings.Members.Descriptors
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct BindableMembersTargetDescriptor<TTarget> : IBindableMembersTargetDescriptor<TTarget> where TTarget : class
    {
        public readonly TTarget Target;

        public BindableMembersTargetDescriptor(TTarget target)
        {
            Should.NotBeNull(target, nameof(target));
            Target = target;
        }

        public BindableMembersDescriptor<TTarget> Descriptor => default;
    }
}