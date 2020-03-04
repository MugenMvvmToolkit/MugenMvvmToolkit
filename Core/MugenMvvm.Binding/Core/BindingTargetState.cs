using System.Runtime.InteropServices;

namespace MugenMvvm.Binding.Core
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct BindingTargetState
    {
        #region Fields

        public readonly object Target;

        #endregion

        #region Constructors

        public BindingTargetState(object target)
        {
            Target = target;
        }

        #endregion

        #region Properties

        public bool IsEmpty => Target == null;

        #endregion
    }
}