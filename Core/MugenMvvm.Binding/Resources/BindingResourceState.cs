using System.Runtime.InteropServices;

namespace MugenMvvm.Binding.Resources
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct BindingResourceState
    {
        #region Fields

        public readonly object? Source;
        public readonly object Target;
        public readonly object? State;

        #endregion

        #region Constructors

        public BindingResourceState(object target, object? source, object? state = null)
        {
            Should.NotBeNull(target, nameof(target));
            Target = target;
            Source = source;
            State = state;
        }

        #endregion

        #region Properties

        public bool IsEmpty => Target == null;

        #endregion
    }
}