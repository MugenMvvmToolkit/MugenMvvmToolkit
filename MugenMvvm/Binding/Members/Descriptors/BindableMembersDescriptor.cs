using System.Runtime.InteropServices;

namespace MugenMvvm.Binding.Members.Descriptors
{
    [StructLayout(LayoutKind.Auto)]
    public readonly ref struct BindableMembersDescriptor<TTarget> where TTarget : class
    {
    }
}