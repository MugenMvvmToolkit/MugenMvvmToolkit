using System.Runtime.InteropServices;

namespace MugenMvvm.Binding.Members.Descriptors
{
    [StructLayout(LayoutKind.Auto)]
    public readonly ref struct BindableMembersTargetDescriptor<TTarget> where TTarget : class
    {
        #region Fields

        public readonly TTarget Target;

        #endregion

        #region Constructors

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