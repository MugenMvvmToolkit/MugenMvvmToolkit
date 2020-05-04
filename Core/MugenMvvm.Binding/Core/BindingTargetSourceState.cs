using System.Runtime.InteropServices;

namespace MugenMvvm.Binding.Core
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct BindingTargetSourceState
    {
        #region Fields

        public readonly object Target;
        public readonly object? Source;

        #endregion

        #region Constructors

        public BindingTargetSourceState(object target, object? source)
        {
            Should.NotBeNull(target, nameof(target));
            Target = target;
            Source = source;
        }

        #endregion

        #region Properties

        public bool IsEmpty => Target == null;

        #endregion
    }
}