using System.Runtime.InteropServices;

namespace MugenMvvm.Binding.Resources
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct ResourceResolverResult
    {
        #region Fields

        public readonly bool IsResolved;
        public readonly object? Resource;

        #endregion

        #region Constructors

        public ResourceResolverResult(object? resource)
        {
            Resource = resource;
            IsResolved = true;
        }

        #endregion
    }
}