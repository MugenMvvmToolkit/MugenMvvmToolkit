using System.Runtime.InteropServices;

namespace MugenMvvm.Binding.Resources
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct ResourceResolverRequest
    {
        #region Fields

        public readonly object? Source;
        public readonly object Target;

        #endregion

        #region Constructors

        public ResourceResolverRequest(object target, object? source)
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