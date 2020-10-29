using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MugenMvvm.Bindings.Members.Descriptors
{
    [StructLayout(LayoutKind.Auto)]
    public readonly ref struct BindableMembersTargetDescriptor<TTarget> where TTarget : class
    {
        #region Fields

        public readonly TTarget Target;

        #endregion

        #region Constructors

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BindableMembersTargetDescriptor(TTarget target)
        {
            Should.NotBeNull(target, nameof(target));
            Target = target;
        }

        #endregion

        #region Properties

        public BindableMembersDescriptor<TTarget> Descriptor => default;

        #endregion
    }
}