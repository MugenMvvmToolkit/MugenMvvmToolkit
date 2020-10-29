using System.Runtime.InteropServices;

namespace MugenMvvm.Bindings.Members.Descriptors
{
    [StructLayout(LayoutKind.Auto)]
    public readonly ref struct BindableMembersDescriptor<TTarget> where TTarget : class
    {
    }
}